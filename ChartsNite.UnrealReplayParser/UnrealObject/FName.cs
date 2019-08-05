using ChartsNite.UnrealReplayParser.StreamArchive;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChartsNite.UnrealReplayParser.UnrealObject
{
    public static class FName
    {
        static Dictionary<int, string> _names;
        public static IReadOnlyDictionary<int, string> FNames => _names;
        static FName()
        {
            _names = new Dictionary<int, string>
            {
                // Special zero value, meaning no name.
                { 0, "None" },

                // Class property types (name indices are significant for serialization).
                { 1, "ByteProperty" },
                { 2, "IntProperty" },
                { 3, "BoolProperty" },
                { 4, "FloatProperty" },
                { 5, "ObjectProperty" }, // ClassProperty shares the same tag
                { 6, "NameProperty" },
                { 7, "DelegateProperty" },
                { 8, "DoubleProperty" },
                { 9, "ArrayProperty" },
                { 10, "StructProperty" },
                { 11, "VectorProperty" },
                { 12, "RotatorProperty" },
                { 13, "StrProperty" },
                { 14, "TextProperty" },
                { 15, "InterfaceProperty" },
                { 16, "MulticastDelegateProperty" },
                //_names.Add(17,Available)
                { 18, "LazyObjectProperty" },
                { 19, "SoftObjectProperty" }, // SoftClassProperty shares the same tag
                { 20, "UInt64Property" },
                { 21, "UInt32Property" },
                { 22, "UInt16Property" },
                { 23, "Int64Property" },
                { 25, "Int16Property" },
                { 26, "Int8Property" },
                //_names.Add(27,Available)
                { 28, "MapProperty" },
                { 29, "SetProperty" },

                // Special packages.
                { 30, "Core" },
                { 31, "Engine" },
                { 32, "Editor" },
                { 33, "CoreUObject" },

                // More class properties
                { 34, "EnumProperty" },

                // Special types.
                { 50, "Cylinder" },
                { 51, "BoxSphereBounds" },
                { 52, "Sphere" },
                { 53, "Box" },
                { 54, "Vector2D" },
                { 55, "IntRect" },
                { 56, "IntPoint" },
                { 57, "Vector4" },
                { 58, "Name" },
                { 59, "Vector" },
                { 60, "Rotator" },
                { 61, "SHVector" },
                { 62, "Color" },
                { 63, "Plane" },
                { 64, "Matrix" },
                { 65, "LinearColor" },
                { 66, "AdvanceFrame" },
                { 67, "Pointer" },
                { 68, "Double" },
                { 69, "Quat" },
                { 70, "Self" },
                { 71, "Transform" },

                // Object class names.
                { 100, "Object" },
                { 101, "Camera" },
                { 102, "Actor" },
                { 103, "ObjectRedirector" },
                { 104, "ObjectArchetype" },
                { 105, "Class" },
                { 106, "ScriptStruct" },
                { 107, "Function" },

                // Misc.
                { 200, "State" },
                { 201, "TRUE" },
                { 202, "FALSE" },
                { 203, "Enum" },
                { 204, "Default" },
                { 205, "Skip" },
                { 206, "Input" },
                { 207, "Package" },
                { 208, "Groups" },
                { 209, "Interface" },
                { 210, "Components" },
                { 211, "Global" },
                { 212, "Super" },
                { 213, "Outer" },
                { 214, "Map" },
                { 215, "Role" },
                { 216, "RemoteRole" },
                { 217, "PersistentLevel" },
                { 218, "TheWorld" },
                { 219, "PackageMetaData" },
                { 220, "InitialState" },
                { 221, "Game" },
                { 222, "SelectionColor" },
                { 223, "UI" },
                { 224, "ExecuteUbergraph" },
                { 225, "DeviceID" },
                { 226, "RootStat" },
                { 227, "MoveActor" },
                { 230, "All" },
                { 231, "MeshEmitterVertexColor" },
                { 232, "TextureOffsetParameter" },
                { 233, "TextureScaleParameter" },
                { 234, "ImpactVel" },
                { 235, "SlideVel" },
                { 236, "TextureOffset1Parameter" },
                { 237, "MeshEmitterDynamicParameter" },
                { 238, "ExpressionInput" },
                { 239, "Untitled" },
                { 240, "Timer" },
                { 241, "Team" },
                { 242, "Low" },
                { 243, "High" },
                { 244, "NetworkGUID" },
                { 245, "GameThread" },
                { 246, "RenderThread" },
                { 247, "OtherChildren" },
                { 248, "Location" },
                { 249, "Rotation" },
                { 250, "BSP" },
                { 251, "EditorSettings" },
                { 252, "AudioThread" },
                { 253, "ID" },
                { 254, "UserDefinedEnum" },
                { 255, "Control" },
                { 256, "Voice" },
                { 257, "Zlib" },
                { 258, "Gzip" },

                // Online
                { 280, "DGram" },
                { 281, "Stream" },
                { 282, "GameNetDriver" },
                { 283, "PendingNetDriver" },
                { 284, "BeaconNetDriver" },
                { 285, "FlushNetDormancy" },
                { 286, "DemoNetDriver" },
                { 287, "GameSession" },
                { 288, "PartySession" },
                { 289, "GamePort" },
                { 290, "BeaconPort" },
                { 291, "MeshPort" },
                { 292, "MeshNetDriver" },
                { 293, "LiveStreamVoice" },

                // Texture settings.
                { 300, "Linear" },
                { 301, "Point" },
                { 302, "Aniso" },
                { 303, "LightMapResolution" },

                // Sound.
                //_names.Add(310,)
                { 311, "UnGrouped" },
                { 312, "VoiceChat" },

                // Optimized replication.
                { 320, "Playing" },
                { 322, "Spectating" },
                { 325, "Inactive" },

                // Log messages.
                { 350, "PerfWarning" },
                { 351, "Info" },
                { 352, "Init" },
                { 353, "Exit" },
                { 354, "Cmd" },
                { 355, "Warning" },
                { 356, "Error" },

                // File format backwards-compatibility.
                { 400, "FontCharacter" },
                { 401, "InitChild2StartBone" },
                { 402, "SoundCueLocalized" },
                { 403, "SoundCue" },
                { 404, "RawDistributionFloat" },
                { 405, "RawDistributionVector" },
                { 406, "InterpCurveFloat" },
                { 407, "InterpCurveVector2D" },
                { 408, "InterpCurveVector" },
                { 409, "InterpCurveTwoVectors" },
                { 410, "InterpCurveQuat" },

                { 450, "AI" },
                { 451, "NavMesh" },

                { 500, "PerformanceCapture" },

                // Special config names - not required to be consistent for network replication
                { 600, "EditorLayout" },
                { 601, "EditorKeyBindings" },
                { 602, "GameUserSettings" }
            };
        }
        public enum FNameId
        {
            None = 0,

            // Class property types (name indices are significant for serialization).
            ByteProperty = 1,
            IntProperty = 2,
            BoolProperty = 3,
            FloatProperty = 4,
            ObjectProperty = 5, // ClassProperty shares the same tag
            NameProperty = 6,
            DelegateProperty = 7,
            DoubleProperty = 8,
            ArrayProperty = 9,
            StructProperty = 10,
            VectorProperty = 11,
            RotatorProperty = 12,
            StrProperty = 13,
            TextProperty = 14,
            InterfaceProperty = 15,
            MulticastDelegateProperty = 16,
            //_names.Add(17,Available)
            LazyObjectProperty = 18,
            SoftObjectProperty = 19, // SoftClassProperty shares the same tag
            UInt64Property = 20,
            UInt32Property = 21,
            UInt16Property = 22,
            Int64Property = 23,
            Int16Property = 25,
            Int8Property = 26,
            //_names.Add(27,Available)
            MapProperty = 28,
            SetProperty = 29,

            // Special packages.
            Core = 30,
            Engine = 31,
            Editor = 32,
            CoreUObject = 33,

            // More class properties
            EnumProperty = 34,

            // Special types.
            Cylinder = 50,
            BoxSphereBounds = 51,
            Sphere = 52,
            Box = 53,
            Vector2D = 54,
            IntRect = 55,
            IntPoint = 56,
            Vector4 = 57,
            Name = 58,
            Vector = 59,
            Rotator = 60,
            SHVector = 61,
            Color = 62,
            Plane = 63,
            Matrix = 64,
            LinearColor = 65,
            AdvanceFrame = 66,
            Pointer = 67,
            Double = 68,
            Quat = 69,
            Self = 70,
            Transform = 71,

            // Object class names.
            Object = 100,
            Camera = 101,
            Actor = 102,
            ObjectRedirector = 103,
            ObjectArchetype = 104,
            Class = 105,
            ScriptStruct = 106,
            Function = 107,

            // Misc.
            State = 200,
            TRUE = 201,
            FALSE = 202,
            Enum = 203,
            Default = 204,
            Skip = 205,
            Input = 206,
            Package = 207,
            Groups = 208,
            Interface = 209,
            Components = 210,
            Global = 211,
            Super = 212,
            Outer = 213,
            Map = 214,
            Role = 215,
            RemoteRole = 216,
            PersistentLevel = 217,
            TheWorld = 218,
            PackageMetaData = 219,
            InitialState = 220,
            Game = 221,
            SelectionColor = 222,
            UI = 223,
            ExecuteUbergraph = 224,
            DeviceID = 225,
            RootStat = 226,
            MoveActor = 227,
            All = 230,
            MeshEmitterVertexColor = 231,
            TextureOffsetParameter = 232,
            TextureScaleParameter = 233,
            ImpactVel = 234,
            SlideVel = 235,
            TextureOffset1Parameter = 236,
            MeshEmitterDynamicParameter = 237,
            ExpressionInput = 238,
            Untitled = 239,
            Timer = 240,
            Team = 241,
            Low = 242,
            High = 243,
            NetworkGUID = 244,
            GameThread = 245,
            RenderThread = 246,
            OtherChildren = 247,
            Location = 248,
            Rotation = 249,
            BSP = 250,
            EditorSettings = 251,
            AudioThread = 252,
            ID = 253,
            UserDefinedEnum = 254,
            Control = 255,
            Voice = 256,
            Zlib = 257,
            Gzip = 258,

            // Online
            DGram = 280,
            Stream = 281,
            GameNetDriver = 282,
            PendingNetDriver = 283,
            BeaconNetDriver = 284,
            FlushNetDormancy = 285,
            DemoNetDriver = 286,
            GameSession = 287,
            PartySession = 288,
            GamePort = 289,
            BeaconPort = 290,
            MeshPort = 291,
            MeshNetDriver = 292,
            LiveStreamVoice = 293,

            // Texture settings.
            Linear = 300,
            Point = 301,
            Aniso = 302,
            LightMapResolution = 303,

            // Sound.
            //_names.Add(310,)
            UnGrouped = 311,
            VoiceChat = 312,

            // Optimized replication.
            Playing = 320,
            Spectating = 322,
            Inactive = 325,

            // Log messages.
            PerfWarning = 350,
            Info = 351,
            Init = 352,
            Exit = 353,
            Cmd = 354,
            Warning = 355,
            Error = 356,

            // File format backwards-compatibility.
            FontCharacter = 400,
            InitChild2StartBone = 401,
            SoundCueLocalized = 402,
            SoundCue = 403,
            RawDistributionFloat = 404,
            RawDistributionVector = 405,
            InterpCurveFloat = 406,
            InterpCurveVector2D = 407,
            InterpCurveVector = 408,
            InterpCurveTwoVectors = 409,
            InterpCurveQuat = 410,

            AI = 450,
            NavMesh = 451,

            PerformanceCapture = 500,

            // Special config names - not required to be consistent for network replication
            EditorLayout = 600,
            EditorKeyBindings = 601,
            GameUserSettings = 602
        }
        public static string GetName( FNameId id ) => FNames[(int)id];
    }
}
