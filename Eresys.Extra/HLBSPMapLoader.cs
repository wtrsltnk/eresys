using Eresys.Math;
using Eresys.Practises.Logging;
using System;
using System.Collections;
using System.IO;
using static Eresys.HlBspMap;

namespace Eresys.Extra
{
    public class HlBspMapLoader
    {
        public ILogger Logger { get; set; } = new ConsoleLogger();

        public void LoadFromFile(string fileName, HlBspMap map, IGraphics graphics)
        {
            map.BSPRendering = true;
            map.FrustumCulling = true;
            map.FileName = fileName;
            Load(map, graphics);
        }

        private void Load(HlBspMap map, IGraphics graphics)
        {
            FileStream fs;                      // stream used to open the bsp file
            BinaryReader br;                        // reader used to read from it

            int i, j, k;                // indices
            int x, gx, y;               // coordinate variables. gx = Global X
            Point3D p;                      // 3D point
            int n, count;               // to count stuff ... (n = Number)
            char[] cstr;                    // a classic null-terminated character array string
            bool found;                 // indicates if we found something ...
            string s, t;                    // some strings

            BspLump[] lumps;                   // A lump is a chunk of data in a BSP file containing a specific type of information, like vertices for example
            BspLump lump;                  // used for easy acces to the info in the lumps array
            ArrayList wads;                 // list of wad files being used by this bsp file
            Point3D[] vertices;             // stores all vertices
            Vertex[] tempVertices;          // a temporary list with all the vertices used by a single face.
            Queue faceVertices;         // a temporary list with all the vertices used by faces. this data will be moved to a vertexPoolI later on.
            Vertex vertex;                  // used to create and modify vertices in the vertices, faceVertices
            BspEdge[] edges;                   // edges (a line between 2 vertices)
            int[] faceEdges;                // indices into the edge array that indicate the edges used by a face
            HlBspMap.BspTexture[] textures;             // texture array. contains only info like size and name
            BspTextureInfo[] textureInfo;          // texture info array. contains info needed to calculate texture coordinates
            Eresys.Texture texture;             // texture object
            int texInfoI;               // index into the textureInfo array
            Point2D minTexCoor, maxTexCoor; // smallest and bigest texture coordinate
            float radius;                   // used to calculate the radius of a face
            ArrayList addedTextures;            // list of all textures that are added. this is used to prevent loading textures multiple times
            byte[] lightData;               // lightmap data
            ArrayList lightmaps;                // contains some info for the lightmaps, like size
            BspLightmap lightmap;              // used for easy acces to elements in the lightmaps list
            int lmw, lmh;               // size of a lightmap
            int lmOffset;               // offset into the lightData array
            int firstEdge, numEdges;    // first edge and number of edges of a face
            int e, v;                   // edge (e) and vertex (v) index
            BspModel[] models;                 // models array
            ScriptBlock[] modelScripts;         // code blocks in the entityScript containing model info
            string model;                   // model identifier
            byte mode, amount;          // render mode and render amount. indicates alpha blending and the amount of alpha blending
            float range;                    // used to determine the size of the skybox

            // open file
            fs = new FileStream(map.FileName, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);

            // check bsp version
            if (br.ReadInt32() != BSPVersion) throw new BspVersionException(map.FileName);

            // load lumps info
            lumps = new BspLump[(int)BspLumpTypes.Count];
            for (i = 0; i < (int)BspLumpTypes.Count; i++)
            {
                lumps[i].offset = br.ReadInt32();
                lumps[i].length = br.ReadInt32();
            }

            // load entity script
            lump = lumps[(int)BspLumpTypes.Entities];
            br.BaseStream.Position = lump.offset;
            cstr = br.ReadChars(lump.length);
            n = 0;
            while (cstr[n] != 0) n++;
            s = new string(cstr, 0, n);
            map.EntityScript = new Script(s);

            // create wads
            i = 0;
            s = map.EntityScript["worldspawn"][0]["wad"];
            wads = new ArrayList();
            while (i < s.Length)
            {
                j = s.IndexOf(';', i);
                if (j < 0) j = s.Length;
                t = s.Substring(i, j - i);
                k = t.LastIndexOf('\\');
                if (k < 0) k = t.LastIndexOf('/');
                if (k >= 0) t = t.Substring(k + 1);
                try
                {
                    wads.Add(new Wad3(t));
                }
                catch (Exception)
                {
                    Logger.Log(LogLevels.Warning, $"Couldn't load wad \'{t}\'!");
                }
                i = j + 1;
            }

            // load planes
            lump = lumps[(int)BspLumpTypes.Planes];
            if (lump.length % 20 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 20;
            map.Planes = new Plane[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                map.Planes[i] = new Plane();
                map.Planes[i].a = br.ReadSingle();
                map.Planes[i].c = br.ReadSingle();
                map.Planes[i].b = br.ReadSingle();
                map.Planes[i].d = br.ReadSingle();
                br.BaseStream.Position += 4;
            }

            // load vertices
            lump = lumps[(int)BspLumpTypes.Vertices];
            if (lump.length % 12 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 12;
            br.BaseStream.Position = lump.offset;
            vertices = new Point3D[n];
            for (i = 0; i < n; i++)
            {
                vertices[i].x = br.ReadSingle();
                vertices[i].z = br.ReadSingle();
                vertices[i].y = br.ReadSingle();
            }

            // load edges
            lump = lumps[(int)BspLumpTypes.Edges];
            if (lump.length % 4 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 4;
            edges = new BspEdge[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                edges[i].v1 = br.ReadUInt16();
                edges[i].v2 = br.ReadUInt16();
            }

            // load face edges
            lump = lumps[(int)BspLumpTypes.SurfaceEdges];
            if (lump.length % 4 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 4;
            faceEdges = new int[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                faceEdges[i] = br.ReadInt32();
            }

            // load textures
            lump = lumps[(int)BspLumpTypes.Textures];
            br.BaseStream.Position = lump.offset;
            n = br.ReadInt32();
            textures = new HlBspMap.BspTexture[n];
            for (i = 0, j = lump.offset + 4; i < n; i++, j += 4)
            {
                br.BaseStream.Position = j;
                textures[i] = new HlBspMap.BspTexture(br, lump.offset, br.ReadInt32());
            }

            // load texture info
            lump = lumps[(int)BspLumpTypes.TexInfo];
            if (lump.length % 40 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 40;
            textureInfo = new BspTextureInfo[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                textureInfo[i].axisU.x = br.ReadSingle();
                textureInfo[i].axisU.z = br.ReadSingle();
                textureInfo[i].axisU.y = br.ReadSingle();
                textureInfo[i].offsetU = br.ReadSingle();
                textureInfo[i].axisV.x = br.ReadSingle();
                textureInfo[i].axisV.z = br.ReadSingle();
                textureInfo[i].axisV.y = br.ReadSingle();
                textureInfo[i].offsetV = br.ReadSingle();
                textureInfo[i].texture = br.ReadInt32();
                br.BaseStream.Position += 4;
            }

            // load lighting data
            lump = lumps[(int)BspLumpTypes.Lighting];
            br.BaseStream.Position = lump.offset;
            lightData = br.ReadBytes(lump.length);

            // load faces
            lump = lumps[(int)BspLumpTypes.Faces];
            if (lump.length % 20 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 20;
            map.Faces = new BspFace[n];
            addedTextures = new ArrayList();
            lightmaps = new ArrayList();
            faceVertices = new Queue();
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                // create new face
                map.Faces[i] = new BspFace();
                map.Faces[i].index = i;
                map.Faces[i].model = false;
                map.Faces[i].alpha = 255;
                map.Faces[i].textureAlpha = false;

                // load in some data
                map.Faces[i].plane = br.ReadInt16();
                br.BaseStream.Position += 2;
                firstEdge = br.ReadInt32();
                numEdges = (int)br.ReadInt16();
                texInfoI = (int)br.ReadInt16();
                br.BaseStream.Position += 4;
                lmOffset = br.ReadInt32();

                // create texture

                // get texture info
                BspTextureInfo texInfo = textureInfo[texInfoI];
                HlBspMap.BspTexture tex = textures[texInfo.texture];

                // is it a sky or a trigger texture?
                if (tex.name == "sky" || tex.name == "aaatrigger")
                {
                    map.Faces[i].texture = -1;
                }
                else
                {
                    texture = null;

                    // look if we didn't already add this texture
                    found = false;
                    for (j = 0; j < addedTextures.Count; j += 2)
                    {
                        texture = (Eresys.Texture)addedTextures[j];
                        if (texture.Name == tex.name)
                        {
                            found = true;
                            break;
                        }
                    }

                    // If we haven't found the texture, create a new one
                    if (!found)
                    {
                        j = 0;
                        texture = null;
                        while (j < wads.Count)
                        {
                            try
                            {
                                texture = ((Wad3)wads[j]).ToTexture(tex.name);
                                break;
                            }
                            catch (Exception)
                            {
                                j++;
                            }
                        }
                        if (texture == null)
                        {
                            // subsititute
                            texture = new Eresys.Texture(1, 1, tex.name);
                            texture[0] = new Color(255, 255, 255);

                            Logger.Log(LogLevels.Warning, $"Couldn't load texture \'{tex.name}\'!");
                        }

                        // add new texture to addedTextures and upload it to the graphics hardware
                        addedTextures.Add(texture);
                        addedTextures.Add(graphics.AddTexture(texture));
                    }

                    // set face texture index
                    map.Faces[i].texture = (int)addedTextures[addedTextures.IndexOf(texture) + 1];
                }

                // create face vertices
                map.Faces[i].firstVertex = faceVertices.Count;
                map.Faces[i].numVertices = numEdges;
                tempVertices = new Vertex[numEdges];
                k = 0;
                minTexCoor.x = minTexCoor.y = System.Single.MaxValue;
                maxTexCoor.x = maxTexCoor.y = System.Single.MinValue;
                map.Faces[i].center.x = 0.0f;
                map.Faces[i].center.y = 0.0f;
                map.Faces[i].center.z = 0.0f;
                for (j = 0; j < numEdges; j++)
                {
                    e = faceEdges[j + firstEdge]; // edge index

                    // if e > 0 we should use this edge clockwise, otherwise we use edge -e counterclockwise
                    if (e > 0)
                        v = edges[e].v1; // clockwise: take first vertex of edge
                    else
                        v = edges[-e].v2; // counterclockwise: take second vertex of edge

                    // set up the new vertex
                    vertex = new Vertex();
                    vertex.position.x = vertices[v].x;
                    vertex.position.y = vertices[v].y;
                    vertex.position.z = vertices[v].z;
                    map.Faces[i].center.x += vertex.position.x;
                    map.Faces[i].center.y += vertex.position.y;
                    map.Faces[i].center.z += vertex.position.z;

                    // calculate its texture coordinate
                    vertex.texCoord.x = (vertex.position.x * texInfo.axisU.x + vertex.position.y * texInfo.axisU.y + vertex.position.z * texInfo.axisU.z + texInfo.offsetU);
                    vertex.texCoord.y = (vertex.position.x * texInfo.axisV.x + vertex.position.y * texInfo.axisV.y + vertex.position.z * texInfo.axisV.z + texInfo.offsetV);
                    if (vertex.texCoord.x > maxTexCoor.x) maxTexCoor.x = vertex.texCoord.x;
                    if (vertex.texCoord.y > maxTexCoor.y) maxTexCoor.y = vertex.texCoord.y;
                    if (vertex.texCoord.x < minTexCoor.x) minTexCoor.x = vertex.texCoord.x;
                    if (vertex.texCoord.y < minTexCoor.y) minTexCoor.y = vertex.texCoord.y;

                    // temporary set lightmap coords to tex coords
                    vertex.lightCoord.x = vertex.texCoord.x;
                    vertex.lightCoord.y = vertex.texCoord.y;

                    // scale texture coordinates to texture space
                    vertex.texCoord.x /= (float)tex.width;
                    vertex.texCoord.y /= (float)tex.height;

                    // add vertex to temporary array
                    tempVertices[k++] = vertex;
                }
                map.Faces[i].center.x /= (float)map.Faces[i].numVertices;
                map.Faces[i].center.y /= (float)map.Faces[i].numVertices;
                map.Faces[i].center.z /= (float)map.Faces[i].numVertices;
                map.Faces[i].radius = 0.0f;
                for (j = 0; j < numEdges; j++)
                {
                    p.x = tempVertices[j].position.x - map.Faces[i].center.x;
                    p.y = tempVertices[j].position.y - map.Faces[i].center.y;
                    p.z = tempVertices[j].position.z - map.Faces[i].center.z;
                    radius = (float)System.Math.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
                    if (radius > map.Faces[i].radius) map.Faces[i].radius = radius;
                }

                // create lightmap
                if (lmOffset < 0)
                {
                    map.Faces[i].lightmap = -1;
                }
                else
                {
                    // calculate lightmap dimensions
                    lmw = (int)(System.Math.Ceiling(maxTexCoor.x / 16.0f) - System.Math.Floor(minTexCoor.x / 16.0f) + 1.0);
                    lmh = (int)(System.Math.Ceiling(maxTexCoor.y / 16.0f) - System.Math.Floor(minTexCoor.y / 16.0f) + 1.0);

                    // calculate lightmap coordinates (in pixels for now, they will later be translated to texture space)
                    for (j = 0; j < tempVertices.Length; j++)
                    {
                        vertex = tempVertices[j];
                        vertex.lightCoord.x = ((lmw * 16.0f) + 2.0f * vertex.lightCoord.x - minTexCoor.x - maxTexCoor.x) / 32.0f;
                        vertex.lightCoord.y = ((lmh * 16.0f) + 2.0f * vertex.lightCoord.y - minTexCoor.y - maxTexCoor.y) / 32.0f;
                        tempVertices[j] = vertex;
                    }

                    // create new lightmap
                    lightmaps.Add(new BspLightmap(lmw, lmh, lmOffset, i));
                }

                // add vertices to faceVertices queue
                for (j = 0; j < tempVertices.Length; j++) faceVertices.Enqueue(tempVertices[j]);
            }

            // create vertexPool
            map.VertexPool = new VertexPool(faceVertices.Count);
            for (i = 0; i < map.VertexPool.Size; i++) map.VertexPool[i] = (Vertex)faceVertices.Dequeue();

            // process lightmap data
            //   for enhanced performance we will group multiple lightmaps of more or less similar height together into one
            //   texture. Therefore we first sort them (hint: the Lightmap class implements IComparable for this reason).

            lightmaps.Sort();

            lmh = 2; // lmh = LightMap Height. We start with all maps less then or equal to 2

            // repeat the following operation until all lightmaps are in a texture. Once added to a texture, we will remove
            // a lightmap from the lightmaps array.

            while (lightmaps.Count > 0)
            {
                lmw = 0;
                i = 0;
                count = 0;

                // this loop checks how many lightmaps are suited to be added to our texture
                while (i < lightmaps.Count)
                {
                    lightmap = (BspLightmap)lightmaps[i++];
                    if (lightmap.height > lmh) break;
                    lmw += lightmap.width;
                    if (lmw > 1024)
                    {
                        lmw -= lightmap.width;
                        break;
                    }
                    count++;
                }
                if (count == 0)
                {
                    lmh = lmh << 1;
                    continue;
                }
                i = 1;
                while (i < lmw) i = i << 1;
                lmw = i;
                texture = new Eresys.Texture(lmw, lmh);
                gx = 0;
                for (i = 0; i < count; i++)
                {
                    lightmap = (BspLightmap)lightmaps[i];
                    j = lightmap.offset;
                    for (y = 0; y < lmh; y++)
                    {
                        for (x = 0; x < lightmap.width; x++)
                        {
                            if (y < lightmap.height)
                            {
                                try
                                {
                                    texture[gx + x, y] = new Color(lightData[j++], lightData[j++], lightData[j++]);
                                }
                                catch (Exception)
                                {
                                    texture[gx + x, y] = new Color(255, 255, 255);
                                }
                            }
                            else
                            {
                                texture[gx + x, y] = new Color(255, 255, 255);
                            }
                        }
                    }
                    n = map.Faces[lightmap.face].firstVertex + map.Faces[lightmap.face].numVertices;
                    for (j = map.Faces[lightmap.face].firstVertex; j < n; j++)
                    {
                        vertex = map.VertexPool[j];
                        vertex.lightCoord.x = (vertex.lightCoord.x + gx) / lmw;
                        vertex.lightCoord.y = vertex.lightCoord.y / lmh;
                        map.VertexPool[j] = vertex;
                    }

                    gx += lightmap.width;
                }
                j = graphics.AddTexture(texture);
                for (i = 0; i < count; i++) map.Faces[((BspLightmap)lightmaps[i]).face].lightmap = j;
                lightmaps.RemoveRange(0, count);
            }

            // load markFaces
            lump = lumps[(int)BspLumpTypes.MarkSurfaces];
            if (lump.length % 2 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 2;
            map.MarkFaces = new ushort[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                map.MarkFaces[i] = br.ReadUInt16();
            }

            // load nodes
            lump = lumps[(int)BspLumpTypes.Nodes];
            if (lump.length % 24 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 24;
            map.Nodes = new BspNode[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                map.Nodes[i] = new BspNode();
                map.Nodes[i].plane = br.ReadInt32();
                map.Nodes[i].frontChild = br.ReadInt16();
                map.Nodes[i].backChild = br.ReadInt16();
                br.BaseStream.Position += 16;
            }

            // load leaves
            lump = lumps[(int)BspLumpTypes.Leaves];
            if (lump.length % 28 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 28;
            map.Leaves = new BspLeaf[n];
            br.BaseStream.Position = lump.offset + 4;
            for (i = 0; i < n; i++)
            {
                map.Leaves[i] = new BspLeaf();
                map.Leaves[i].pvs = br.ReadInt32();
                br.BaseStream.Position += 12;
                map.Leaves[i].firstMarkFace = br.ReadUInt16();
                map.Leaves[i].numMarkFaces = br.ReadUInt16();
                br.BaseStream.Position += 8;
            }

            // load visibility data (PVS sets)
            lump = lumps[(int)BspLumpTypes.VisDate];
            br.BaseStream.Position = lump.offset;
            map.Pvs = br.ReadBytes(lump.length);

            // load models
            lump = lumps[(int)BspLumpTypes.Models];
            if (lump.length % 64 != 0)
            {
                br.Close();
                fs.Close();
                throw new BspCorruptedException(map.FileName);
            }
            n = lump.length / 64;
            models = new BspModel[n];
            br.BaseStream.Position = lump.offset;
            for (i = 0; i < n; i++)
            {
                br.BaseStream.Position += 24;
                models[i].origin.x = br.ReadSingle();
                models[i].origin.z = br.ReadSingle();
                models[i].origin.y = br.ReadSingle();
                br.BaseStream.Position += 20;
                models[i].firstFace = br.ReadInt32();
                models[i].numFaces = br.ReadInt32();
            }

            // process models
            modelScripts = map.EntityScript["func_wall;func_illusionary;func_water;func_breakable"];
            for (i = 0; i < modelScripts.Length; i++)
            {
                try
                {
                    model = modelScripts[i]["model"];
                    mode = Byte.Parse(modelScripts[i]["rendermode"]);
                    amount = Byte.Parse(modelScripts[i]["renderamt"]);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
                if (model[0] != '*') continue;
                j = Int32.Parse(model.Substring(1));
                for (k = 0; k < models[j].numFaces; k++)
                {
                    BspFace face = map.Faces[k + models[j].firstFace];
                    face.model = true;
                    face.textureAlpha = mode == 4;
                    face.alpha = amount;
                    for (v = 0; v < face.numVertices; v++)
                    {
                        vertex = map.VertexPool[v + face.firstVertex];
                        vertex.position.x += models[j].origin.x;
                        vertex.position.y += models[j].origin.y;
                        vertex.position.z += models[j].origin.z;
                        map.VertexPool[v + face.firstVertex] = vertex;
                    }
                }
            }

            // upload vertex pool to the graphics hardware
            map.VertexPoolIndex = graphics.AddVertexPool(map.VertexPool);

            // create model faces array
            map.ModelFaces = new ArrayList();
            for (i = 0; i < map.Faces.Length; i++)
            {
                BspFace face = map.Faces[i];
                if (face.texture < 0) continue;
                if (face.model)
                {
                    map.ModelFaces.Add(face);
                }
            }

            // create skybox
            try
            {
                range = Int32.Parse(map.EntityScript["worldspawn"][0]["MaxRange"]);
            }
            catch (Exception)
            {
                range = 0x4000;
            }
            string skyname;
            try
            {
                skyname = map.EntityScript["worldspawn"][0]["skyname"] + ".bmp";
            }
            catch (Exception)
            {
                skyname = "";
            }

            Eresys.Texture skyTexture = null;
            if (skyname != "")
            {
                try
                {
                    skyTexture = new Eresys.Texture(skyname);
                }
                catch (Exception)
                {
                    Logger.Log(LogLevels.Warning, $"Couldn't load sky texture \'{skyname }\'!");

                    try
                    {
                        skyTexture = new Eresys.Texture("defaultsky.bmp");
                    }
                    catch (Exception)
                    {
                        Logger.Log(LogLevels.Warning, $"Couldn't load default sky texture \'defaultsky.bmp\'!");

                        skyTexture = new Eresys.Texture(1, 1);
                        skyTexture[0] = new Color(132, 184, 255);
                    }
                }
            }
            if (skyTexture != null)
            {
                map.Sky = new SkyBox(graphics, range, range, range, skyTexture);
            }
            else
            {
                map.Sky = null;
            }

            // check face plane oriëntations
            for (i = 0; i < map.Faces.Length; i++)
            {
                BspFace face = map.Faces[i];
                Vector vector1 = map.VertexPool[face.firstVertex].position - face.center;
                Vector vector2 = map.VertexPool[face.firstVertex + 1].position - face.center;
                vector1 = vector1.Cross(vector2);
                vector1.Normalize();
                face.inversePlane = vector1.Dot(map.Planes[face.plane].GetNormal()) < 0.0f;
            }

            // close streams
            br.Close();
            fs.Close();
        }
    }
}
