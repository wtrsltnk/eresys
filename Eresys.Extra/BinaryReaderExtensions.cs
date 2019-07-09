using Eresys.Math;
using System;
using System.IO;
using System.Linq;
using static Eresys.HlBspMap;

namespace Eresys.Extra
{

    internal static class BinaryReaderExtensions
    {
        public static BspLump[] ReadBspLumps(this BinaryReader br, int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(index => new BspLump()
                {
                    offset = br.ReadInt32(),
                    length = br.ReadInt32(),
                })
                .ToArray();
        }

        public static string ReadStringOfLength(this BinaryReader br, int length)
        {
            return new string(br
                .ReadChars(length)
                .TakeWhile(c => c != 0)
                .ToArray());
        }

        public static T[] ReadLumpData<T>(this BinaryReader br, BspLump lump, int size, Func<BinaryReader, T> readType)
        {
            if (lump.length % size != 0)
            {
                throw new BspCorruptedException();
            }

            int count = lump.length / size;
            if (count <= 0)
            {
                throw new BspCorruptedException();
            }

            br.BaseStream.Position = lump.offset;

            return Enumerable
                .Range(0, count)
                .Select(i => readType(br))
                .ToArray();
        }

        public static Plane ReadPlane(this BinaryReader br)
        {
            var result = new Plane();

            result.a = br.ReadSingle();
            result.c = br.ReadSingle();
            result.b = br.ReadSingle();
            result.d = br.ReadSingle();
            br.BaseStream.Position += 4;

            return result;
        }

        public static Point3D ReadPoint3D(this BinaryReader br)
        {
            var result = new Point3D();

            result.x = br.ReadSingle();
            result.z = br.ReadSingle();
            result.y = br.ReadSingle();

            return result;
        }

        public static BspEdge ReadBspEdge(this BinaryReader br)
        {
            var result = new BspEdge();

            result.v1 = br.ReadUInt16();
            result.v2 = br.ReadUInt16();

            return result;
        }

        public static BspTextureInfo ReadBspTextureInfo(this BinaryReader br)
        {
            var result = new BspTextureInfo();

            result.axisU.x = br.ReadSingle();
            result.axisU.z = br.ReadSingle();
            result.axisU.y = br.ReadSingle();
            result.offsetU = br.ReadSingle();
            result.axisV.x = br.ReadSingle();
            result.axisV.z = br.ReadSingle();
            result.axisV.y = br.ReadSingle();
            result.offsetV = br.ReadSingle();
            result.texture = br.ReadInt32();
            br.BaseStream.Position += 4;

            return result;
        }

        public static BspNode ReadBspNode(this BinaryReader br)
        {
            var result = new BspNode();

            result.plane = br.ReadInt32();
            result.frontChild = br.ReadInt16();
            result.backChild = br.ReadInt16();
            br.BaseStream.Position += 16;

            return result;
        }

        public static BspLeaf ReadBspLeaf(this BinaryReader br)
        {
            var result = new BspLeaf();

            result.pvs = br.ReadInt32();
            br.BaseStream.Position += 12;
            result.firstMarkFace = br.ReadUInt16();
            result.numMarkFaces = br.ReadUInt16();
            br.BaseStream.Position += 8;

            return result;
        }

        public static BspModel ReadBspModel(this BinaryReader br)
        {
            var result = new BspModel();

            br.BaseStream.Position += 24;
            result.origin.x = br.ReadSingle();
            result.origin.z = br.ReadSingle();
            result.origin.y = br.ReadSingle();
            br.BaseStream.Position += 20;
            result.firstFace = br.ReadInt32();
            result.numFaces = br.ReadInt32();

            return result;
        }
    }
}
