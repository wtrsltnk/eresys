using Eresys.Extra;
using Eresys.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Eresys
{
    public class HlBspMap : SceneObject
    {
        #region Consts
        public const int BSPVersion = 30;
        #endregion

        #region Public Members
        public override bool Solid => true;

        public bool BSPRendering { get; set; }

        public bool FrustumCulling { get; set; }
        #endregion

        #region Public Methods

        public override void Render(IGraphics graphics, CameraSceneObject camera)
        {
            int i, j;
            float d;
            Plane plane;
            bool[] pvs;
            ArrayList visFaces;
            BspFace face;
            bool visible;
            bool firstAlphaFace;

            Profiler.StartSample("HLBSPMap.Render");

            // create visible faces list
            visFaces = new ArrayList();

            // add faces to visible faces list
            if (BSPRendering)
            {
                // get camera node
                Profiler.StartSample("get camera leaf");
                i = 0;
                while (i >= 0)
                {
                    plane = Planes[Nodes[i].plane];
                    d = plane.Distance(camera.Position);
                    if (d >= 0.0f) i = Nodes[i].frontChild;
                    else i = Nodes[i].backChild;
                }
                Profiler.StopSample();

                // get pvs for current leaf (~i => -(i + 1))
                Profiler.StartSample("decompress pvs");
                i = Leaves[~i].pvs;
                pvs = DecompressPvs(i);
                Profiler.StopSample();

                // add visible faces
                Profiler.StartSample("add visible faces");
                for (i = 0; i < Leaves.Length; i++) // go through all leaves
                {
                    visible = i == 0;
                    if (!visible) visible = pvs[i - 1];
                    if (visible) // if leaf is visible
                    {
                        for (j = 0; j < Leaves[i].numMarkFaces; j++) // go through all faces of leaf ..
                        {
                            face = Faces[MarkFaces[Leaves[i].firstMarkFace + j]];
                            if (face.texture < 0) continue;
                            if (face.model) continue;
                            visFaces.Add(face); // .. and add them
                        }
                    }
                }
                Profiler.StopSample();

                // add all model faces
                Profiler.StartSample("add model faces");
                visFaces.AddRange(ModelFaces);
                Profiler.StopSample();
            }
            else
            {
                Profiler.StartSample("add all faces");
                for (i = 0; i < Faces.Length; i++)
                {
                    face = Faces[i];
                    if (face.texture < 0) continue;
                    visFaces.Add(face);
                }
                Profiler.StopSample();
            }

            // perform frustum culling on visible faces
            if (FrustumCulling)
            {
                Profiler.StartSample("frustum culling");
                for (i = visFaces.Count - 1; i >= 0; i--)
                {
                    face = (BspFace)visFaces[i];
                    if (!camera.InsideFrustum(face.center, face.radius)) visFaces.RemoveAt(i);
                }
                Profiler.StopSample();
            }

            // update camera distance of visible faces
            Profiler.StartSample("update face distance");
            for (i = 0; i < visFaces.Count; i++) ((BspFace)visFaces[i]).UpdateCamDist(camera);
            Profiler.StopSample();

            // sort visible faces
            Profiler.StartSample("sort faces");
            visFaces.Sort();
            Profiler.StopSample();

            // render visible faces + sky somewher ebetween the transparant and the solid faces
            Profiler.StartSample("render faces");
            firstAlphaFace = false;
            for (i = 0; i < visFaces.Count; i++)
            {
                face = (BspFace)visFaces[i];
                if (face.IsTransparant())
                {
                    if (!firstAlphaFace && Sky != null)
                    {
                        // render skybox
                        Profiler.StartSample("render sky");
                        Sky.Render(graphics, camera);
                        Profiler.StopSample();
                        firstAlphaFace = true;
                    }
                    graphics.AlphaBlending = true;
                    graphics.Alpha = face.alpha;
                    graphics.TextureAlpha = face.textureAlpha;
                }
                else
                {
                    graphics.AlphaBlending = false;
                }
                graphics.Lighting = face.lightmap >= 0;
                graphics.RenderTriangleFan(VertexPoolIndex, face.firstVertex, face.numVertices, face.texture, face.lightmap);
            }
            if (!firstAlphaFace && Sky != null)
            {
                // render skybox
                Profiler.StartSample("render sky");
                Sky.Render(graphics, camera);
                Profiler.StopSample();
            }
            Profiler.StopSample();

            Profiler.StopSample();
        }

        private bool[] DecompressPvs(int offset)
        {
            int i, v, c, n;
            bool[] res;

            v = offset;
            n = Leaves.Length - 1;
            res = new bool[n];

            c = 0;
            while (c < n)
            {
                if (v < 0 || v >= Pvs.Length)
                {
                    res[c++] = true;
                    continue;
                }
                if (Pvs[v] == 0) // 0 = repeater byte: next byte contains number of following bytes that are 0.
                {
                    v++;
                    i = Pvs[v] << 3; // -> x << 3 = x*2^3 = x*8
                    while (i > 0 && c < n)
                    {
                        i--;
                        res[c] = false;
                        c++;
                    }
                    v++;
                    continue;
                }

                // check every bit of the curent byte if it is set
                for (i = 0; i < 8 && c < n; i++, c++) res[c] = (Pvs[v] & (1 << i)) != 0;
                v++;
            }

            return res;
        }

        private Point3D ClosestPointOnLine(Point3D a, Point3D b, Point3D p)
        {
            // Determine t (the length of the vector from ‘a’ to ‘p’)

            Vector c = p - a;
            Vector v = b - a;
            float d = v.Magnitude();
            v.Normalize();
            float t = v.Dot(c);

            // Check to see if ‘t’ is beyond the extents of the line segment

            if (t < 0) return a;
            if (t > d) return b;

            // Return the point between ‘a’ and ‘b’

            v *= t;
            return a + v;
        }

        private Point3D ClosestPointOnFace(int faceIdx, Point3D p)
        {
            BspFace face = Faces[faceIdx];
            float mag = float.MaxValue;
            Point3D res = p;
            for (int i = 0; i < face.numVertices; i++)
            {
                int prevI = i - 1;
                if (prevI < 0) prevI += face.numVertices;
                Point3D q = ClosestPointOnLine(VertexPool[prevI + face.firstVertex].position, VertexPool[i + face.firstVertex].position, p);
                float d = ((Vector)(q - p)).Magnitude();
                if (d < mag)
                {
                    res = q;
                    mag = d;
                }
            }
            return res;
        }

        private bool PointInFace(int faceIdx, Point3D point)
        {
            BspFace face = Faces[faceIdx];
            Vector v, w;
            w = VertexPool[face.firstVertex + face.numVertices - 1].position - point;
            w.Normalize();
            float angle = 0.0f;
            for (int i = 0; i < face.numVertices; i++)
            {
                v = w;
                w = VertexPool[i + face.firstVertex].position - point;
                w.Normalize();
                angle += (float)System.Math.Acos(v.Dot(w));
            }
            return angle > 6.2828f;
        }

        private Collision CheckFace(int faceIdx, Point3D start, Vector movement, float startFraction, float endFraction, float sphere)
        {
            BspFace face = Faces[faceIdx];
            //Plane plane = planes[face.plane];

            // get accurate plane
            Plane plane = Planes[face.plane];
            if (face.inversePlane)
            {
                plane = new Plane(-plane.a, -plane.b, -plane.c, -plane.d);
            }

            // calculate distance of start- and endpoint to plane
            float ds = plane.Distance(start + movement * startFraction);
            float de = plane.Distance(start + movement * endFraction);

            // if the starting point of the sphere lies behind the plane, no collision should occur, because in this case the plane faces away from us and is supposed to be invisible
            if (ds < sphere) return null;

            // check if we are moving away from the plane (with our back towards the front of the plane) or parallel to the plane
            // in neither of these cases a collision will occur
            if (ds <= de) return null;

            // finally, if this check fails we know for sure there is a collision
            // it tests if the endpoint is on or beyond the plane
            if (de >= sphere) return null;

            // calculate fraction of the movement vector where we hit the plane
            float fraction = (ds - sphere) / (ds - de);

            // apply a window to this fraction value, just to be sure it is valid
            if (fraction < 0.0f) fraction = 0.0f;
            if (fraction > 1.0f) fraction = 1.0f;

            // scale this fraction to fit on the real movement vector
            fraction = startFraction + fraction * (endFraction - startFraction);

            // we now know for sure we intersect the plane, but do we intersect it inside the face that lies on top of it?
            // to find that out we first need to calculate an intersection point
            Point3D intersection = start + movement * fraction - plane.GetNormal() * sphere;
            if (!PointInFace(faceIdx, intersection))
            {
                // if this point doesnt lie within the face, perhaps a part of our sphere does
                // to check this we get the closest point on one of the edges of the face to our intersection point
                Point3D closestPoint = ClosestPointOnFace(faceIdx, intersection);
                // and then we check the distance between those 2 points to se if a part of our sphere does hit the face
                Vector distance = intersection - closestPoint;
                if (distance.MagnitudeSquare() > sphere * sphere) return null;
            }

            return new Collision(fraction, plane.GetNormal());
        }

        private Collision ClosestCollision(Collision c1, Collision c2)
        {
            Collision res;
            if (c1 == null && c2 == null) res = null;
            else if (c1 == null) res = c2;
            else if (c2 == null) res = c1;
            else if (System.Math.Abs(c1.fraction - c2.fraction) < collisionMargin)
            {
                // when 2 collision are almost equal, we merge them
                c1.fraction = System.Math.Min(c1.fraction, c2.fraction);
                c1.response += c2.response;
                c1.response.Normalize();
                res = c1;
            }
            else if (c1.fraction < c2.fraction) res = c1;
            else res = c2;
            if (res == null) return null;
            // adjust small and big fraction values so we won't get ridiculously many microscopic collisions no one would ever notice
            if (res.fraction < 1.0f) res.fraction = 0.0f;
            if (res.fraction > 0.9f) res.fraction = 1.0f;
            return res;
        }

        private Collision CheckLeaf(int leafIdx, Point3D start, Vector movement, float startFraction, float endFraction, float sphere)
        {
            BspLeaf leaf = Leaves[leafIdx];
            Collision c = null;
            for (int i = 0; i < leaf.numMarkFaces; i++)
            {
                c = ClosestCollision(c, CheckFace(MarkFaces[leaf.firstMarkFace + i], start, movement, startFraction, endFraction, sphere));
            }
            return c;
        }

        private Collision CheckNode(int nodeIdx, Point3D start, Vector movement, float startFraction, float endFraction, float sphere)
        {
            // calculate start and end point
            Point3D s = start + movement * startFraction;
            Point3D e = start + movement * endFraction;

            // if this node is a leaf, check for collision within that leaf
            if (nodeIdx < 0)
            {
                return CheckLeaf(~nodeIdx, start, movement, startFraction, endFraction, sphere);
            }

            // get node
            BspNode node = Nodes[nodeIdx];

            // get splitting plane
            Plane plane = Planes[node.plane];

            // calculate distance of start and end point to plane
            float ds = plane.Distance(s);
            float de = plane.Distance(e);

            if (ds > sphere && de > sphere)
            {
                // both points are in front of the plane
                return CheckNode(node.frontChild, start, movement, startFraction, endFraction, sphere);
            }
            else if (ds < -sphere && de < -sphere)
            {
                // both points are behind the plane
                return CheckNode(node.backChild, start, movement, startFraction, endFraction, sphere);
            }
            else
            {
                float f1, f2;

                float ads = System.Math.Abs(ds);
                float ade = System.Math.Abs(de);
                float ad = ads + ade;
                float frac = endFraction - startFraction;

                f1 = frac * (ads + sphere) / ad + collisionMargin;
                f2 = frac * (ade + sphere) / ad + collisionMargin;

                Collision c1 = CheckNode(node.frontChild, start, movement, startFraction, startFraction + f1, sphere);
                Collision c2 = CheckNode(node.backChild, start, movement, endFraction - f2, endFraction, sphere);

                return ClosestCollision(c1, c2);
            }
        }

        public override Vector CheckCollision(Point3D start, Vector movement, float sphere)
        {
            if (movement.IsNullVector()) return movement;

            Vector res = new Vector();
            int numCollisions = 0;

            for (; ; )
            {
                Collision collision = null;
                //for(int i = 0; i < faces.Length; i ++) collision = ClosestCollision(collision, CheckFace(i, start, movement, 0.0f, 1.0f, sphere));
                collision = CheckNode(0, start, movement, 0.0f, 1.0f, sphere);
                for (int i = 0; i < ModelFaces.Count; i++) collision = ClosestCollision(collision, CheckFace(((BspFace)ModelFaces[i]).index, start, movement, 0.0f, 1.0f, sphere));

                if (collision == null) break;
                if (collision.fraction == 1.0f) break;

                float movMag = movement.Magnitude();
                movement.Normalize();

                float fracMag = collision.fraction * movMag - collisionMargin;
                if (fracMag < 0.0f) fracMag = 0.0f;
                res += movement * fracMag;

                numCollisions++;
                if (numCollisions == maxCollisions)
                {
                    movement = new Vector();
                    break;
                }

                start += res;

                movement = collision.response.Cross(movement).Cross(collision.response) * (1.0f - collision.fraction) * movMag;
            }

            res += movement;

            return res;
        }

        #endregion

        #region Bsp Types

        public enum BspLumpTypes
        {
            Entities = 0,
            Planes,
            Textures,
            Vertices,
            VisDate,
            Nodes,
            TexInfo,
            Faces,
            Lighting,
            ClipNodes,
            Leaves,
            MarkSurfaces,
            Edges,
            SurfaceEdges,
            Models,
            Count
        }

        public struct BspLump
        {
            public int offset;
            public int length;
        }

        public struct BspEdge
        {
            public ushort v1; // indices into the vertex array
            public ushort v2; //
        }

        public class BspTexture
        {
            public string name;
            public uint width, height;

            public BspTexture(BinaryReader stream, int lumpOffset, int texOffset)
            {
                char[] cstr;
                int l;

                stream.BaseStream.Position = lumpOffset + texOffset;
                cstr = stream.ReadChars(16);
                l = 0;
                while (cstr[l] != 0) l++;
                name = new string(cstr, 0, l);
                width = stream.ReadUInt32();
                height = stream.ReadUInt32();
            }
        }

        public struct BspTextureInfo
        {
            public Point3D axisU;       // These 2 axes define a plane. Texture coordinates are calculated by projecting
            public float offsetU;   // the vertices of a face onto this plane. This makes aligning textures to faces
                                    // a lot easier.
            public Point3D axisV;       //      Magic forumula:
            public float offsetV;   //      u = x*axisU.x + y*axisU.y + z*axisU.z + offsetU (dito for v..)
            public int texture;
        }

        public class BspLightmap : System.IComparable
        {
            public int width;
            public int height;
            public int offset;
            public int face;

            public BspLightmap(int width, int height, int offset, int face)
            {
                this.width = width;
                this.height = height;
                this.offset = offset;
                this.face = face;
            }

            public int CompareTo(object obj)
            {
                return height - ((BspLightmap)obj).height;
            }
        }

        public class BspFace : IComparable
        {
            public int index;
            public short plane;
            public bool inversePlane;
            public int firstVertex;
            public int numVertices;
            public int texture;
            public int lightmap;
            public float camDist;
            public Point3D center;
            public float radius;
            public byte alpha;
            public bool textureAlpha;
            public bool model;

            public bool IsTransparant()
            {
                return alpha < 255 || textureAlpha;
            }

            public void UpdateCamDist(CameraSceneObject camera)
            {
                float dx, dy, dz;

                dx = center.x - camera.Position.x;
                dy = center.y - camera.Position.y;
                dz = center.z - camera.Position.z;

                camDist = (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }

            public int CompareTo(object obj)
            {
                BspFace face;
                float dist;

                face = (BspFace)obj;

                if (IsTransparant())
                {
                    if (face.IsTransparant())
                    {
                        dist = face.camDist - camDist;
                        if (dist == 0.0f) return 0;
                        if (dist > 0.0f) return 1;
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if (face.IsTransparant())
                    {
                        return -1;
                    }
                    else
                    {
                        dist = face.camDist - camDist;
                        if (dist == 0.0f) return 0;
                        if (dist > 0.0f) return -1;
                        return 1;
                    }
                }
            }
        }

        public class BspNode
        {
            public int plane;       // index of the splitting plane (in the plane array)
            public short frontChild;    // index of the front child node or leaf (leaf = -(child + 1))
            public short backChild; // index of the back child node or leaf (leaf = -(child + 1))
        }

        public class BspLeaf
        {
            public int pvs;         // index into the pvs array; -1 = no pvs (--> everything is visible)
            public ushort firstMarkFace;    // index of the first face (in the face leaf array)
            public ushort numMarkFaces; // number of consecutive edges (in the face leaf array)
        }

        public struct BspModel
        {
            public Point3D origin;
            public int firstFace, numFaces;
        }

        #endregion

        #region BspData

        private static float collisionMargin = 1.0f / 32.0f;
        private static int maxCollisions = 100;

        public string FileName { get; set; }            // name of the bsp file

        public Script EntityScript { get; set; }              // the entity script included with the bsp file

        public Plane[] Planes { get; set; }             // planes array

        public VertexPool VertexPool { get; set; }          // the vertex pool

        public int VertexPoolIndex { get; set; }        // vertexPool index

        public BspFace[] Faces { get; set; }               // faces array

        public ushort[] MarkFaces { get; set; }         // markFaces array. these are indices into the faces array

        public List<BspFace> ModelFaces { get; set; }           // stores all faces used in models

        public BspNode[] Nodes { get; set; }               // nodes array. these are the nodes of the bsp tree

        public BspLeaf[] Leaves { get; set; }              // leaves array. these are the leaves of the bsp tree

        public byte[] Pvs { get; set; }             // PVS (= Potential Visibility Set) array. This is used to determine if a leaf is visible

        public SkyBox Sky { get; set; }             // class for rendering background sky

        #endregion
    }
}
