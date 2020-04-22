
namespace Iviz.Msgs.visualization_msgs
{
    public sealed class InteractiveMarker : IMessage
    {
        // Time/frame info.
        // If header.time is set to 0, the marker will be retransformed into
        // its frame on each timestep. You will receive the pose feedback
        // in the same frame.
        // Otherwise, you might receive feedback in a different frame.
        // For rviz, this will be the current 'fixed frame' set by the user.
        public std_msgs.Header header;
        
        // Initial pose. Also, defines the pivot point for rotations.
        public geometry_msgs.Pose pose;
        
        // Identifying string. Must be globally unique in
        // the topic that this message is sent through.
        public string name;
        
        // Short description (< 40 characters).
        public string description;
        
        // Scale to be used for default controls (default=1).
        public float scale;
        
        // All menu and submenu entries associated with this marker.
        public MenuEntry[] menu_entries;
        
        // List of controls displayed for this marker.
        public InteractiveMarkerControl[] controls;
    
        /// <summary> Full ROS name of this message. </summary>
        public const string MessageType = "visualization_msgs/InteractiveMarker";
    
        public IMessage Create() => new InteractiveMarker();
    
        public int GetLength()
        {
            int size = 76;
            size += header.GetLength();
            size += name.Length;
            size += description.Length;
            for (int i = 0; i < menu_entries.Length; i++)
            {
                size += menu_entries[i].GetLength();
            }
            for (int i = 0; i < controls.Length; i++)
            {
                size += controls[i].GetLength();
            }
            return size;
        }
    
        /// <summary> Constructor for empty message. </summary>
        public InteractiveMarker()
        {
            header = new std_msgs.Header();
            name = "";
            description = "";
            menu_entries = new MenuEntry[0];
            controls = new InteractiveMarkerControl[0];
        }
        
        public unsafe void Deserialize(ref byte* ptr, byte* end)
        {
            header.Deserialize(ref ptr, end);
            pose.Deserialize(ref ptr, end);
            BuiltIns.Deserialize(out name, ref ptr, end);
            BuiltIns.Deserialize(out description, ref ptr, end);
            BuiltIns.Deserialize(out scale, ref ptr, end);
            BuiltIns.DeserializeArray(out menu_entries, ref ptr, end, 0);
            BuiltIns.DeserializeArray(out controls, ref ptr, end, 0);
        }
    
        public unsafe void Serialize(ref byte* ptr, byte* end)
        {
            header.Serialize(ref ptr, end);
            pose.Serialize(ref ptr, end);
            BuiltIns.Serialize(name, ref ptr, end);
            BuiltIns.Serialize(description, ref ptr, end);
            BuiltIns.Serialize(scale, ref ptr, end);
            BuiltIns.SerializeArray(menu_entries, ref ptr, end, 0);
            BuiltIns.SerializeArray(controls, ref ptr, end, 0);
        }
    
        /// <summary> MD5 hash of a compact representation of the message. </summary>
        public const string Md5Sum = "dd86d22909d5a3364b384492e35c10af";
    
        /// <summary> Base64 of the GZip'd compression of the concatenated dependencies file. </summary>
        public const string DependenciesBase64 =
                "H4sIAAAAAAAACr1a+28bNxL+uQLyPxA2ithXWX6luVZ3/kGx5ESoX2crfaAoBEpLSWxWyw1317Ly1983" +
                "Q3Ifkt0Uh0uSIN7lksOZ4Ty+GXpXjPRSHc6sXCqhk5nptHbFcCYWSkbKdnJ8FDoTmcpFbsRRW+QLJZbS" +
                "flBWrHQci4kSVuVWJtnM2KWKQCQ3oKHzTDiqJhFKTheCaGW5SjviN1O4xVZNlX5QTDQ1mRIzpaKJnH4g" +
                "AgkPZ0SCCRFnNxiyK52ptliDyFLPF3lJJSympVJEejZTViV5tfrCWGEf9CeSAkIF/mmbaWF57suZfoQQ" +
                "vOQliz1Z84Qigzpa71gtXjstUlWicy1j5r4jenFm2iJSM52ozEmlH0yOr9CKgIKENbnMtUmyTmuuzBKa" +
                "W4+X2Tw7vCXxiQpTjcCLnq11MhdZbvGjI66KDMwoMY/NRMbxWhSJ/ljQoWEBbZWbVE/xJHMnHrSdybk/" +
                "PmyfL6wp5otOy1EUCUSkze4XxuZgOptanRJvYu/f4tWRmC6kldNc2Wy/XFObxUunMqaNiS8oKGIRIb4s" +
                "4lxMTZJbE2diz4+cHYPQLDYyPz0RGS0lGj0cwlIlhZBJJLJiws/g12qoUGaZmWqZg/RK5wsvGJtfp3WF" +
                "mQNMXP/+B1MY+1VE9VJDW2ZWMRHpLI3l2vPYoDNMICQkhQ1d8dC5WwSyYXnrRevs//znRevq/m0Xxxs5" +
                "A3Cm9YK0mkMV0kYQKpeRzCWzvICtK3sQqwcVY5VcppCFv+brVMGedsWIpMK/uUogENsIHQrOZ2qWS9jL" +
                "FJr0flhb75xNilTaXE+LWFrMNzbSCU0vvQf/MgWLS6ZKDPtd0k2mpgWpDTvpZGqVzMhIhn3RKmDwdMrq" +
                "Y2t3tDIHeFVzeE65uTNUMKseUwtLBTMy62KPfzjhOqAN7SjsEsGEeGyM12xfYBOwoFKDoLIHzm/X+QJW" +
                "Sz7wIK2WE9gkCMPAYlB9SYte7tcoE9tdmH9iAnlHsdrj75BNSrok08ECZxazvxZzKBATU2sedISpPoJM" +
                "Y01uGOuJlXbd4tDKW7Z2LzhQslHyiegtyw8OyKcx1tGXM8jtsEQ22UOUpXOCBBy/yLc4YkNRM6sgSSqn" +
                "iMowNBqO/HfNc8mzDTzTr+2I1i0HxDCh9Z8CgtqE6Vbzvp6MYOZF8B9yeakTH76DCBAHDsJcNyR28ez1" +
                "K/FYPq3Lp09fS4JKf6UY5XHBlBpabfJPbx8r7VMO77Q+I1R4Wn058R50VshYf2KOnYxltCcRy5eQ5cD0" +
                "rhgQzNiK5mUiXEjWhbRWrulAt4hwEO3BAuDh03DsYRalo9zMFSEQnweR9SRWUO459JmLfoZHlSMdQ9ME" +
                "NyjzkkWtsT25vbEMlWBUs1jmIMJcdQQOD4GAjye3xTQvMLt+mi6czPUDBQOGVWW+XBOVhKJvUiwnYJLc" +
                "ToodhHV8RszYETOt4og2MalPJGWixT5gDyQMQRdOtuVCcSaOsIo00ViAHAGE5QKUg2vagoKjXIK3FbRd" +
                "Sh6WeHTnnY2ECQq45V1BJuy0JOAj01QhrE4ULJQ51UhSCx1HmOzO/hGRNFaUQQ4Es3yKR9EUgkdynSOS" +
                "n4mdWZHsVNNPPjfdmonJawtebS04aS5I7Ult+vefm46zxhNWYPgtrBc6dofL5hDrDy5DkISgCebdA7PF" +
                "lPCCLcOjI8doss8JJZgFgYkNy6HD9fDSo8rqSEHAo6C2g2iw5TgSCQ6ZzgOWEZI9spLbDV7DwZNldW+M" +
                "R8N2baFnKBAcV95bwMLQTfQe5aa6OXnDXrGegHmlSqpMSi7K4VYgdOhJsaZDInUv5O92Xiw5TnqcRELq" +
                "JGKwxN7mB8eEs8TeRMVmtR/I+G9E6Lw+jT3c5RB+hxLwnhJkouCh2W0WKqlU4y2eJQZUmH5QEdcsg0H/" +
                "Te/8J8JC5M/JdoC7CHVPCHTsvSUiXpOKfAHHu/HgSyBF3uDu5v7u/XUXOIzAnCI7zyw8w1GRpXYQc8Bu" +
                "6bdOTxxQxJ6cmAe176ld9t5fn79rEoxlkUwX/xNNOtYfSjWcHfkBx/bZcfXq9j078SP1U/uqieq5coLy" +
                "1l0tKwe3KpGwq6OqQqXMNk5rpKiSsi9eni4WqxLHb0EnQ0V3AkuhbQAu9ZySj/f2Bxnj/2Af+LlVUs+s" +
                "WfIpvX0/5CQB6wXXxAxsuqyQRaosYYiMZtQ5EHuqgyJWikmR5/jGFr6/UYuCcL9WO8cGWHurFBF7ViFl" +
                "ctvAVJ2DRszhimZTV/tkaKuFRuQDXxNF+3I5TqUH7I6bGKDs/K7vatauV2++3qjXn4GsJMNNDWwtTaS6" +
                "VRW6MKsGFkONnXjcMbx+N7gbjrroUsTxxjQI98TZIzgMfx2gEvtJqbQx3zUxyKp8ewJ1Rk6R/Ofh4Jfx" +
                "Re98eA3b7cVkBOuDTwIGl/iwAVyj4JN7j12yohXKULRaurGa5W3xqVukpUd6filPiuCkxA4GglPWduN0" +
                "54drnI5JP66P4sXzOtsyYVeAXt9cD7oVTifDwqNJUIAulfQ9loZP/kskFNZB/jHnmEi6vhpcv++iR4CU" +
                "ShTbZJXCUjPpgA3T5Q1KPaxwWvLm/Wh0gzA5iBXFLpSBCXkrKcatcWZzdfPzYNz7dXgPLoM9CRkb2Joz" +
                "58cD+aizcurtZY8lKufCRt3E8lxcWB31RoHwHRstwqgpYLZPknXzu5STJuxP1WZs6zV64TxJEfXDJB3V" +
                "ztLJz8fovwdJCWqVi6ptCCCF2FxuxjioNtV9wuhrcL5z2t+p7NwbArCosR9C9we2bxBnvrt/N7wYfXc+" +
                "uruENbmPp31q4mXGVlo47dc1S0UPNypoKhc/Nc3SVK/X7XkCuQgGQtvDduf4QtWtQzdle6QmD1O7KNDY" +
                "en3Qv7lgihHiJ8Uoz04oi0M/sLaFk8LvFE7HywNN/bOpVR77YVun7sOPwedqavU+5/q8KDK4PimhOOKG" +
                "Cy++PyrR0iRDh1NpdECwysMWdCILTT6SoMEJlTQCFHbotCYG7ilj4P9sHJbTti4rbuc7lGdToFAoynnw" +
                "RsuBlHypJLeLCcUs0xyYzlDeYXY2+o6COzIcWylwmBxxmNE4VVhBRO/FrmPrCjIqECca5wQotpl72gyu" +
                "KViUveONznctcfnut8tL2+GbxDlwmBelqesrc5fBd9eDiADLQAuc48KmTAyq4cyVkVRCfD4pPsmDOwtq" +
                "oDqNOLtIGqGbTtNB7gAPyGoIw1MPPjLJS+iDgm91N0Ag3h0vAvGcVFs68BRasRJHrFauodJpnAjLiBpj" +
                "JQo0xkg8HErINTW7AkJXKRAxYX23dryZh/9GY5s0xECmgVUiozI6aoYsO1fAocy5KwDRvaqhg55rtqLx" +
                "Xt9nIrkF5txkM6I91Uv/qo0U1hY3mlFXLvI87R4erlarDnB6x9j54Up/0Id0T3LYd745ogazX8dR6y8X" +
                "jQrUPgAdYUX27Wnv25OjN2gOT/HzfiFBjRP1kmp5unSyS49zEngzV2f1Cww2HSIdglnv7u7ml7ISOH//" +
                "ZlDWAfe3sJRBWQSc/3Y5vO4P7s5O/QBeB+P70d3w9uxVfehyeD86+75G0Y28bpB1YyEA394Mr0f3ZyH2" +
                "jga/jsY1tzn7sUyk9+/Gd4P7m/d352A0sA0eetdvLz3R4+NSuH6/FO3qpj+8+K187Q8uB6NKOPfau7yE" +
                "dM2bKeoAPPVnN3xndNW8+PNnUCLy7DkijtA11rnkCD+HkdADHZqZ/IneGSh2Oh13/QD/hWP9iQKQz5hP" +
                "0xXtdGNAQCaUIVQFeOCnPKFWaC2Ib775C278rug9YMtZET+5KVFNSrbZAGWiU1x45NyAgmFHKlb8UnEg" +
                "CDxYzwfX8s9ygtarr/UbzHvff37VkZBRdIjYgCqOu7W8ui2O6eoMcZ47EfttceL4o85QNem0GkTkdKPZ" +
                "c5eLT2x+W0sUnu3m4p8xZOypu7HbWOwuABurxXEbfxmOZ2VO3isoDAFV0QeoU2RoOltFbRR//XVuYmPv" +
                "3r7h9iuOprkPfxW/H3WODo47R3+0osK6iBHrGQ4MZvOkYt8hjTD2rrHn+1ex5H4XtxNdLSgRt8gH6IJn" +
                "7ZVKzdIjLwtNRc/LpR93D4NED9z/9N6+mxXSud8WGZGXHrilSO4d3Btv36DX7s9p03V5e47EdkPljnMt" +
                "p3jXcUrVVKN3guiMpJnhBBkZeXdaogBHMnMxq10Lg20BR93fshfwAFDA+Tn7Mjvu+nYkX9LidH2jV8FT" +
                "fWORoD0uHAGlPKapVgTGrm+oypFxupABiq6BU4jVJ0yLL3RpK0IHbimXjuXVtSsSPYIJjUKMPTe/Edo3" +
                "F+K8FmPgV1NY3BOx1fAQVo8VBIlwNwjoAm+gZPm1roq8Nz95TyQe+GPzhojxzjDfqLO5R+RX8i9bIEq5" +
                "uz2AOctuRcCVoRQdC/ViJcptzCe4jhZUmoKYbBREbPauU9R2ZQbPYuckLgLUosrI92sq7EA0hZcOFc3s" +
                "xEFJ5tlt5prZoeLa75CLEoJl7IoHXOvwdTrj1sAXX/vmBr/W4esM5mPLWarftkjgpRK9gL++RPvyv09Q" +
                "Wv2L8ncubPk0L58m5ZOECf4XYVi94A8kAAA=";
                
    }
}
