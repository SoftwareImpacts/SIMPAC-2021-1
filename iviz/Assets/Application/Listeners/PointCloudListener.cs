﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iviz.Msgs.sensor_msgs;
using UnityEngine;

namespace Iviz.App
{
    public class PointCloudListener : DisplayableListener
    {
        PointListResource pointCloud;

        public float MinIntensity { get; private set; }
        public float MaxIntensity { get; private set; }
        public int Size { get; private set; } 
        
        public bool CalculateMinMax { get; private set; } = true;

        [Serializable]
        public class Configuration
        {
            public Resource.Module module => Resource.Module.PointCloud;
            public string topic = "";
            public string intensityChannel = "x";
            public float pointSize = 0.03f;
            public Resource.ColormapId colormap = Resource.ColormapId.hsv;
        }

        readonly Configuration config = new Configuration();
        public Configuration Config
        {
            get => config;
            set
            {
                config.topic = value.topic;
                IntensityChannel = value.intensityChannel;
                PointSize = value.pointSize;
                Colormap = value.colormap;
            }
        }

        public string IntensityChannel
        {
            get => config.intensityChannel;
            set
            {
                config.intensityChannel = value;
            }
        }

        public float PointSize
        {
            get => config.pointSize;
            set
            {
                config.pointSize = value;
                pointCloud.Scale = value * Vector2.one;
            }
        }

        public Resource.ColormapId Colormap
        {
            get => config.colormap;
            set
            {
                config.colormap = value;
                pointCloud.Colormap = value;
            }
        }

        readonly List<string> fieldNames = new List<string>() { "x", "y", "z" };
        public IReadOnlyList<string> FieldNames => fieldNames;

        PointWithColor[] pointBuffer = new PointWithColor[0];

        void Awake()
        {
            pointCloud = ResourcePool.GetOrCreate(Resource.Markers.PointList, transform).GetComponent<PointListResource>();

            Config = new Configuration();
            transform.localRotation = Quaternion.identity.ToRos().Ros2Unity();
        }

        public override void StartListening()
        {
            Topic = config.topic;
            Listener = new RosListener<PointCloud2>(config.topic, Handler);
            GameThread.EverySecond += UpdateStats;
        }

        public override void Unsubscribe()
        {
            GameThread.EverySecond -= UpdateStats;
            Listener?.Stop();
            Listener = null;
        }

        static int FieldSizeFromType(int datatype)
        {
            switch (datatype)
            {
                case PointField.FLOAT64:
                    return 8;
                case PointField.FLOAT32:
                case PointField.INT32:
                case PointField.UINT32:
                    return 4;
                case PointField.INT16:
                case PointField.UINT16:
                    return 2;
                case PointField.INT8:
                case PointField.UINT8:
                    return 1;
                default:
                    return -1;
            }
        }

        void Handler(PointCloud2 msg)
        {
            SetParent(msg.header.frame_id);

            if (msg.point_step < 3 * 4 ||
                msg.row_step < msg.point_step * msg.width ||
                msg.data.Length < msg.row_step * msg.height)
            {
                Logger.Info("PointCloudListener: Invalid point cloud dimensions!");
                return;
            }

            fieldNames.Clear();
            fieldNames.AddRange(msg.fields.Select(x => x.name));

            int newSize = (int)(msg.width * msg.height);
            if (newSize > pointBuffer.Length)
            {
                pointBuffer = new PointWithColor[newSize * 11 / 10];
            }


            Task.Run(() =>
            {
                Dictionary<string, PointField> fieldOffsets = new Dictionary<string, PointField>();
                msg.fields.ForEach(x => fieldOffsets.Add(x.name, x));

                if (!fieldOffsets.TryGetValue("x", out PointField xField) || xField.datatype != PointField.FLOAT32 ||
                    !fieldOffsets.TryGetValue("y", out PointField yField) || yField.datatype != PointField.FLOAT32 ||
                    !fieldOffsets.TryGetValue("z", out PointField zField) || zField.datatype != PointField.FLOAT32)
                {
                    Logger.Info("PointCloudListener: Unsupported point cloud! Expected XYZ as floats.");
                    return;
                }
                int xOffset = (int)xField.offset;
                int yOffset = (int)yField.offset;
                int zOffset = (int)zField.offset;

                if (!fieldOffsets.TryGetValue(config.intensityChannel, out PointField iField))
                {
                    iField = new PointField();
                }
                int iOffset = (int)iField.offset;
                int iSize = FieldSizeFromType(iField.datatype);
                if (iSize == -1 || msg.point_step < iOffset + iSize)
                {
                    Logger.Info("PointCloudListener: Invalid or unsupported intensity field type!");
                    return;
                }

                bool rgbaHint = iSize == 4 && (iField.name == "rgb" || iField.name == "rgba");

                GeneratePointBuffer(msg, xOffset, yOffset, zOffset, iOffset, iField.datatype, rgbaHint);

                Vector2 intensityBounds =  rgbaHint ? Vector2.zero : CalculateBounds(newSize);

                GameThread.RunOnce(() =>
                {
                    Size = newSize;
                    pointCloud.IntensityBounds = intensityBounds;
                    pointCloud.UseIntensityTexture = !rgbaHint;
                    pointCloud.SetPointsWithColor(pointBuffer, Size);
                });
            });
        }

        Vector2 CalculateBounds(int size)
        {

            float intensityMin = float.MaxValue, intensityMax = float.MinValue;
            for (int i = 0; i < size; i++)
            {
                Vector3 position = pointBuffer[i].position;
                float intensity = pointBuffer[i].intensity;
                if (float.IsNaN(position.x) ||
                    float.IsNaN(position.y) ||
                    float.IsNaN(position.z) ||
                    float.IsNaN(intensity))
                {
                    continue;
                }

                intensityMin = Mathf.Min(intensityMin, intensity);
                intensityMax = Mathf.Max(intensityMax, intensity);
            }

            return new Vector2(intensityMin, intensityMax);
        }


        void GeneratePointBuffer(PointCloud2 msg, int xOffset, int yOffset, int zOffset, int iOffset, int iType, bool rgbaHint)
        {
            bool xyzAligned = xOffset == 0 && yOffset == 4 && zOffset == 8;
            if (xyzAligned)
            {
                if (rgbaHint)
                {
                    GeneratePointBufferXYZ(msg, iOffset, PointField.FLOAT32);
                } else
                {
                    GeneratePointBufferXYZ(msg, iOffset, iType);
                }
            }
            else
            {
                GeneratePointBufferSlow(msg, xOffset, yOffset, zOffset, iOffset, iType, rgbaHint);
            }
        }

        void GeneratePointBufferSlow(PointCloud2 msg, int xOffset, int yOffset, int zOffset, int iOffset, int iType, bool rgbaHint)
        {
            int heightOffset = 0;
            int pointOffset = 0;
            int rowStep = (int)msg.row_step;
            int pointStep = (int)msg.point_step;

            Func<byte[], int, float> intensityFn;
            if (rgbaHint)
            {
                intensityFn = BitConverter.ToSingle;
            }
            else
            {
                switch (iType)
                {
                    case PointField.FLOAT32:
                        intensityFn = BitConverter.ToSingle;
                        break;
                    case PointField.FLOAT64:
                        intensityFn = (m, o) => (float)BitConverter.ToDouble(m, o);
                        break;
                    case PointField.INT8:
                        intensityFn = (m, o) => (sbyte)m[o];
                        break;
                    case PointField.UINT8:
                        intensityFn = (m, o) => m[o];
                        break;
                    case PointField.INT16:
                        intensityFn = (m, o) => BitConverter.ToInt16(m, o);
                        break;
                    case PointField.UINT16:
                        intensityFn = (m, o) => BitConverter.ToUInt16(m, o);
                        break;
                    case PointField.INT32:
                        intensityFn = (m, o) => BitConverter.ToInt32(m, o);
                        break;
                    case PointField.UINT32:
                        intensityFn = (m, o) => BitConverter.ToUInt32(m, o);
                        break;
                    default:
                        intensityFn = (m, o) => 0;
                        break;
                }
            }

            if (intensityFn != null)
            {
                for (int v = (int)msg.height; v > 0; v--, heightOffset += rowStep)
                {
                    int rowOffset = heightOffset;
                    for (int u = (int)msg.width; u > 0; u--, rowOffset += pointStep, pointOffset++)
                    {
                        Vector3 xyz = new Vector3(
                            BitConverter.ToSingle(msg.data, rowOffset + xOffset),
                            BitConverter.ToSingle(msg.data, rowOffset + yOffset),
                            BitConverter.ToSingle(msg.data, rowOffset + zOffset)
                        );
                        pointBuffer[pointOffset] = new PointWithColor(
                            new Vector3(-xyz.y, xyz.z, xyz.x),
                            intensityFn(msg.data, rowOffset + iOffset)
                        );
                    }
                }
            }
        }

        /*
        void GeneratePointBufferXYZ_float(PointCloud2 msg, int iOffset)
        {
            int rowStep = (int)msg.row_step;
            int pointStep = (int)msg.point_step;
            int height = (int)msg.height;
            int width = (int)msg.width;

            unsafe
            {
                fixed (byte* dataPtr = msg.data)
                fixed (PointWithColor* pointBufferPtr = pointBuffer)
                {
                    PointWithColor* pointBufferOff = pointBufferPtr;
                    byte* dataRow = dataPtr;
                    for (int v = height; v > 0; v--, dataRow += rowStep)
                    {
                        byte* dataOff = dataRow;
                        for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                        {
                            float* datap = (float*)dataOff;
                            float* datai = (float*)(dataOff + iOffset);
                            pointBufferOff->position.x = -datap[1];
                            pointBufferOff->position.y = datap[2];
                            pointBufferOff->position.z = datap[0];
                            pointBufferOff->intensity = *datai;
                        }
                    }
                }
            }
        }

        void GeneratePointBufferXYZ_int(PointCloud2 msg, int iOffset)
        {
            int rowStep = (int)msg.row_step;
            int pointStep = (int)msg.point_step;
            int height = (int)msg.height;
            int width = (int)msg.width;

            unsafe
            {
                fixed (byte* dataPtr = msg.data)
                fixed (PointWithColor* pointBufferPtr = pointBuffer)
                {
                    PointWithColor* pointBufferOff = pointBufferPtr;
                    byte* dataRow = dataPtr;
                    for (int v = height; v > 0; v--, dataRow += rowStep)
                    {
                        byte* dataOff = dataRow;
                        for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                        {
                            float* datap = (float*)dataOff;
                            int* datai = (int*)(dataOff + iOffset);
                            pointBufferOff->position.x = -datap[1];
                            pointBufferOff->position.y = datap[2];
                            pointBufferOff->position.z = datap[0];
                            pointBufferOff->intensity = *datai;
                        }
                    }
                }
            }
        }
        */

        void GeneratePointBufferXYZ(PointCloud2 msg, int iOffset, int iType)
        {
            int rowStep = (int)msg.row_step;
            int pointStep = (int)msg.point_step;
            int height = (int)msg.height;
            int width = (int)msg.width;

            unsafe
            {
                fixed (byte* dataPtr = msg.data)
                fixed (PointWithColor* pointBufferPtr = pointBuffer)
                {
                    PointWithColor* pointBufferOff = pointBufferPtr;
                    byte* dataRow = dataPtr;
                    for (int v = height; v > 0; v--, dataRow += rowStep)
                    {
                        byte* dataOff = dataRow;
                        switch (iType)
                        {
                            case PointField.FLOAT32:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = *(float*)(dataOff + iOffset);
                                }
                                break;
                            case PointField.FLOAT64:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = (float)*(double*)(dataOff + iOffset);
                                }
                                break;
                            case PointField.INT8:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = (sbyte)*(dataOff + iOffset);
                                }
                                break;
                            case PointField.UINT8:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = *(dataOff + iOffset);
                                }
                                break;
                            case PointField.INT16:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = *(short*)(dataOff + iOffset);
                                }
                                break;
                            case PointField.UINT16:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = *(ushort*)(dataOff + iOffset);
                                }
                                break;
                            case PointField.INT32:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = *(int*)(dataOff + iOffset);
                                }
                                break;
                            case PointField.UINT32:
                                for (int u = width; u > 0; u--, dataOff += pointStep, pointBufferOff++)
                                {
                                    float* datap = (float*)dataOff;
                                    pointBufferOff->position.x = -datap[1];
                                    pointBufferOff->position.y = datap[2];
                                    pointBufferOff->position.z = datap[0];
                                    pointBufferOff->intensity = *(uint*)(dataOff + iOffset);
                                }
                                break;
                            default:
                                //??
                                break;
                        }
                    }
                }
            }
        }

        public override void Recycle()
        {
            base.Recycle();
            ResourcePool.Dispose(Resource.Markers.PointList, pointCloud.gameObject);
            pointCloud = null;
        }
    }
}

/*
var hexviz = new Dictionary<string, int[]> {
    {"lines", new[] {0x0072bd, 0xd95319, 0xedb120, 0x7e2f8e, 0x77ac30, 0x4dbeee, 0xa2142f, 0x0072bd, 0xd95319, 0xedb120, 0x7e2f8e, 0x77ac30, 0x4dbeee, 0xa2142f, 0x0072bd, 0xd95319}},
    {"pink", new[] {0x3c0000, 0x653636, 0x814c4c, 0x985d5d, 0xac6c6c, 0xbe7878, 0xc69184, 0xcda68e, 0xd4b898, 0xdac9a1, 0xe1d9aa, 0xe7e7b2, 0xededc8, 0xf3f3dc, 0xf9f9ee, 0xffffff}},
    {"copper", new[] {0x000000, 0x150d08, 0x2b1b11, 0x402819, 0x553522, 0x6a422a, 0x805033, 0x955d3b, 0xaa6a44, 0xbf784c, 0xd48555, 0xea925d, 0xff9f65, 0xffad6e, 0xffba76, 0xffc77f}},
    {"bone", new[] {0x000005, 0x0f0f1a, 0x1e1e2e, 0x2d2d42, 0x3c3c56, 0x4a4a6a, 0x595f79, 0x687388, 0x778797, 0x869ba6, 0x95afb5, 0xa4c3c3, 0xbad2d2, 0xd1e1e1, 0xe8f0f0, 0xffffff}},
    {"gray", new[] {0x000000, 0x111111, 0x222222, 0x333333, 0x444444, 0x555555, 0x666666, 0x777777, 0x888888, 0x999999, 0xaaaaaa, 0xbbbbbb, 0xcccccc, 0xdddddd, 0xeeeeee, 0xffffff}},
    {"winter", new[] {0x0000ff, 0x0011f7, 0x0022ee, 0x0033e6, 0x0044dd, 0x0055d5, 0x0066cc, 0x0077c3, 0x0088bb, 0x0099b3, 0x00aaaa, 0x00bba2, 0x00cc99, 0x00dd91, 0x00ee88, 0x00ff80}},
    {"autumn", new[] {0xff0000, 0xff1100, 0xff2200, 0xff3300, 0xff4400, 0xff5500, 0xff6600, 0xff7700, 0xff8800, 0xff9900, 0xffaa00, 0xffbb00, 0xffcc00, 0xffdd00, 0xffee00, 0xffff00}},
    {"summer", new[] {0x008066, 0x118866, 0x229166, 0x339966, 0x44a266, 0x55aa66, 0x66b366, 0x77bb66, 0x88c366, 0x99cc66, 0xaad466, 0xbbdd66, 0xcce666, 0xddee66, 0xeef766, 0xffff66}},
    {"spring", new[] {0xff00ff, 0xff11ee, 0xff22dd, 0xff33cc, 0xff44bb, 0xff55aa, 0xff6699, 0xff7788, 0xff8877, 0xff9966, 0xffaa55, 0xffbb44, 0xffcc33, 0xffdd22, 0xffee11, 0xffff00}},
    {"cool",new[] { 0x00ffff, 0x11eeff, 0x22ddff, 0x33ccff, 0x44bbff, 0x55aaff, 0x6699ff, 0x7788ff, 0x8877ff, 0x9966ff, 0xaa55ff, 0xbb44ff, 0xbf40ff, 0xdd22ff, 0xee11ff, 0xff00ff}},
    {"hot", new[] {0x2b0000, 0x550000, 0x800000, 0xaa0000, 0xd50000, 0xff0000, 0xff0000, 0xff5500, 0xff8000, 0xffaa00, 0xffd500, 0xffff00, 0xffff40, 0xffff80, 0xffffbf, 0xffffff}},
    {"hsv", new[] {0xff0000, 0xff6000, 0xffbf00, 0xdfff00, 0x80ff00, 0x20ff00, 0x00ff40, 0x00ff9f, 0x00ffff, 0x009fff, 0x0040ff, 0x2000ff, 0x8000ff, 0xdf00ff, 0xff00bf, 0xff0060}},
    {"jet", new[] {0x0000bf, 0x0000ff, 0x0040ff, 0x0080ff, 0x00bfff, 0x00ffff, 0x40ffbf, 0x80ff80, 0xbfff40, 0xffff00, 0xffbf00, 0xff8000, 0xff4000, 0xff0000, 0xbf0000, 0x800000}},
    {"parula", new[] {0x352a87, 0x3145bc, 0x0265e1, 0x0f77db, 0x1388d3, 0x079ccf, 0x07aac1, 0x20b4ad, 0x49bc94, 0x7abf7c, 0xa5be6b, 0xcabb5c, 0xecb94c, 0xfec634, 0xf6dd22, 0xf9fb0e } }
};

Texture2D t = new Texture2D(16, 1);
foreach (var entry in hexviz)
{
    Color32[] pixels = entry.Value.Select(x => new Color32((byte)((x >> 16) & 0xff), (byte)((x >> 8) & 0xff), (byte)((x >> 0) & 0xff), 255)).ToArray();
    t.SetPixels32(pixels);
    byte[] png = t.EncodeToPNG();
    File.WriteAllBytes(entry.Key + ".png", png);
}
*/