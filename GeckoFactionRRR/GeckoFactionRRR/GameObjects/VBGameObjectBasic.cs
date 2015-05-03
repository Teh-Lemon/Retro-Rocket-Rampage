#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GeckoFactionRRR
{
    abstract class VBGameObjectBasic : IGameObject
    {
        //  Graphics Settings
        protected BasicEffect shader;
        protected VertexDeclaration myVertDec;
        // Vertex buffer for fill model
        protected VertexBuffer myVBuffer;
        // Vertex buffer for wireframe
        protected VertexBuffer myVWFBuffer; 
        protected IndexBuffer myIBuffer;

        private GraphicsDevice graphicsDevice;
        private GraphicsDeviceManager graphics;

        protected int noVerts = 0;
        protected int noInd = 0;
        public float modelScale = 1f;

        public string name = "Null";

        public List<Vector3> partOffsets = new List<Vector3>();

        protected VertexPositionColor[] modelVertices;
        protected VertexPositionColor[] wireVertices;
        protected short[] modelIndices;

        public bool HasFillModel = true;
        public bool HasWireFrameModel = true;
        protected PrimitiveType ModelPrimitiveType = PrimitiveType.TriangleList;

        //  Whether to swap x and z values around
        public bool xToZ = false;

        //  World object settings
        //  Size of model XYZ
        public Vector3 dimensions;
        //  Origin of model
        public Vector3 centerPoint = new Vector3(0f, 0f, 0f);

        //  Collision
        //  List of all the triangles, held as their 3 points
        public List<Vector3[]> VectorTrianglesList = new List<Vector3[]>();
        public List<Triangle> Triangles = new List<Triangle>();
        public List<BoundingBox> HitBoxes = new List<BoundingBox>();
        public List<Plane> HitPlanes = new List<Plane>();

        public float modelStat = 100f;

        public float Opacity { get; set; }

        //  DEBUG
        protected BasicEffect basicEffect;

        public VBGameObjectBasic(GraphicsDevice gd, GraphicsDeviceManager gdm
            , string fileName = "", ContentManager content = null)
            : base()
        {
            graphicsDevice = gd;
            graphics = gdm;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;

            //MODEL LOADING
            if (fileName != "")
            {
                LoadModelFromFile(fileName);
            }

            Opacity = 1.0f;
        }

        //  Sets the position of the model and it's collision models
        public void SetPosition(Vector3 newPos)
        {
            //Matrix diffPos = Matrix.CreateTranslation(Position - newPos);

            MoveHitSpheres(1, newPos - Position);

            Position = newPos;
        }

        public virtual void BuildCollisionModels()
        {
            return;
        }

        //  COLLISIONS
        
        //  Returns the largest value out of a XYZ vector
        //  Returns the largest value out of a XYZ vector
        static protected float GetMinDimensions(Vector3 dimensions)
        {
            //  If X is smaller than both Y and Z, return X
            if (dimensions.X < dimensions.Y && dimensions.X < dimensions.Z)
            {
                return dimensions.X;
            }
            else if (dimensions.Y < dimensions.X && dimensions.Y < dimensions.Z)
            {
                return dimensions.Y;
            }
            else
            {
                return dimensions.Z;
            }
        }
        static protected float GetMaxDimensions(Vector3 dimensions)
        {
            //  If X is bigger than both Y and Z, return X
            if (dimensions.X > dimensions.Y && dimensions.X > dimensions.Z)
            {
                return dimensions.X;
            }
            else if (dimensions.Y > dimensions.X && dimensions.Y > dimensions.Z)
            {
                return dimensions.Y;
            }
            else
            {
                return dimensions.Z;
            }
        }

        //  Add a bounding sphere to list of HitSpheres
        public void AddHitSphere(Vector3 position, float diameter)
        {
            HitSpheres.Add(new BoundingSphere(position, diameter / 2));
        }

        //  Translate the bounding spheres by the given vector
        public virtual void MoveHitSpheres(float dt, Vector3 dPosition)
        {
            //  Foreach bounding sphere            
            for (int i = 0; i < HitSpheres.Count; i++)
            {
                //  Translate the sphere
                HitSpheres[i] = HitSpheres[i].Transform(Matrix.CreateTranslation(dPosition * dt));
            }            
        }

        //  Rotates a point round the origin for a given pitch angle
        Vector3 ApplyPitchRotation(Vector3 point, float pitch)
        {
            //  Trigonometry
            float sin = (float)Math.Sin(-pitch);
            float cos = (float)Math.Cos(-pitch);

            float newY = (point.Z * sin) + (point.Y * cos);
            float newZ = (point.Z * cos) - (point.Y * sin);

            return new Vector3(point.X, newY, newZ);
        }

        // Rotates a point round the origin for a given yaw angle
        Vector3 ApplyYawRotation(Vector3 point, float yaw)
        {
            //  Trigonometry
            float sin = (float)Math.Sin(-yaw);
            float cos = (float)Math.Cos(-yaw);

            float newY = (point.Z * sin) + (point.X * cos);
            float newZ = (point.Z * cos) - (point.X * sin);

            return new Vector3(point.X, newY, newZ);
        }

        //  Adds a bounding box to the list of Bounding Boxes
        public void AddHitBox(Vector3 min, Vector3 max)
        {
            //  Create a bounding box for each triangle 
            HitBoxes.Add(new BoundingBox(min, max));
        }

        //  Adds a bounding box and plane for each triangle in the model
        //  Ignored extra triangles used to make a quad face
        public void AddHitBox(Vector3 pos1, Vector3 pos2, Vector3 pos3)
        {
            //  If Box is not unique, don't add it
            bool uniqueBox = true;
            //  Min and Max co-ordinates of bounding box
            Vector3 min;
            Vector3 max;

            //  Find the min and max points of each triangle
            Vector3.Min(ref pos1, ref pos2, out min);
            Vector3.Min(ref min, ref pos3, out min);
            Vector3.Max(ref pos1, ref pos2, out max);
            Vector3.Max(ref max, ref pos3, out max);

            //  The difference between two bounding boxes (one to add and pre-existing)
            float minDiff;
            float maxDiff;

            //  Go through all the pre-existing boxes
            for (int i = 0; i < HitBoxes.Count; i++)
            {
                minDiff = Vector3.Distance(min, HitBoxes[i].Min);
                maxDiff = Vector3.Distance(max, HitBoxes[i].Max);

                //  If the boxes are in the same place don't add the bounding box
                if (minDiff <= 10 && maxDiff <= 10)
                {
                    uniqueBox = false;
                    break;
                }
            }

            AddHitBox(min, max);
        }

        //  Adds a plane to the list of planes
        public void AddHitPlane(Vector3 pos1, Vector3 pos2, Vector3 pos3)
        {
            HitPlanes.Add(new Plane(pos1, pos2, pos3));
        }

        // Populates the list of triangles using the game object's vertices and indices
        // NOTE: UpdateTrianglePositions() MUST BE RUN AFTER GAME OBJECT HAS BEEN TRANSFORMED INTO WORLD SPACE
        public void CreateTriangles()
        {
            //  Counter for triangle list building
            int t = -1;

            // Loop through every index
            for (int i = 0; i < modelIndices.Length; i++)
            {
                // If the start of a new triangle, create a new entry in the list of triangles
                if (i % 3 == 0)
                {
                    VectorTrianglesList.Add(new Vector3[3]);
                    t++;
                }
                // Grab each set of 3 vertices and set them as the 3 points of the triangle
                VectorTrianglesList[t][i % 3] = modelVertices[(short)modelIndices[i]].Position;
            }
        }

        //  Update triangle list to model's current pos/rot/scale
        public void UpdateTrianglesPositions()
        {
            //  Foreach triangle
            for (int i = 0; i < VectorTrianglesList.Count; i++)
            {
                //  Foreach of the 3 points on the triangle
                for (int axis = 0; axis < 3; axis++)
                {
                    // Apply the world transforms
                    VectorTrianglesList[i][axis] *= Scale;
                    VectorTrianglesList[i][axis] = ApplyPitchRotation(VectorTrianglesList[i][axis], Pitch);
                    VectorTrianglesList[i][axis] += Position;
                }
            }
        }

        //  Find the normal of a specified point of triangle
        protected static Vector3 FindNormal(Vector3 pos1, Vector3 pos2, Vector3 pos3)
        {
            Vector3 side1 = pos2 - pos1;
            Vector3 side2 = pos3 - pos1;
            Vector3 cross = Vector3.Cross(side1, side2);
            return Vector3.Normalize(cross);
        }

        //  Change the file colour of the model and outline
        public void ChangeColor(Color modelColor, Color wireColor)
        {            
            for (int i = 0; i < modelVertices.Count(); i++)
            {
                if (modelVertices[i].Color != modelColor)
                {
                    modelVertices[i].Color = modelColor;
                }
                if (wireVertices[i].Color != wireColor)
                {
                    wireVertices[i].Color = wireColor;
                }
            }

            myVBuffer.SetData<VertexPositionColor>(modelVertices);
            myVWFBuffer.SetData<VertexPositionColor>(wireVertices);
        }

        // Build a mesh out of the given vertices and indicies
        protected void BuildModelMesh(List<Vector3> VerticesList, short[] IndicesList)
        {
            #region Vertices
            modelVertices = new VertexPositionColor[VerticesList.Count];
            wireVertices = new VertexPositionColor[VerticesList.Count];

            // Update the game object's vertices
            for (int i = 0; i < VerticesList.Count; i++)
            {
                modelVertices[i] = new VertexPositionColor(VerticesList[i], new Color(0, 0, 0));
                wireVertices[i] = new VertexPositionColor(VerticesList[i], new Color(0, 0, 0));
            }

            //build the buffer after the vertex array is populated
            myVBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), modelVertices.Length
                        , BufferUsage.WriteOnly);
            myVBuffer.SetData<VertexPositionColor>(modelVertices);

            myVWFBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), modelVertices.Length
            , BufferUsage.WriteOnly);
            myVWFBuffer.SetData<VertexPositionColor>(wireVertices);
            #endregion

            #region Indices
            // Update the game object's indices
            modelIndices = IndicesList;
            // Build the indices buffer
            myIBuffer = new IndexBuffer(graphics.GraphicsDevice, typeof(short), modelIndices.Length
                , BufferUsage.WriteOnly);
            myIBuffer.SetData(modelIndices);
            #endregion
        }

        //  Read in model file and build mesh out of it
        //  Build triangleList collision mesh
        public virtual void LoadModelFromFile(string fileName = "")
        {
            modelScale = 1f;

            if (fileName != "")
            {
                //Load all lines from a text file into a string array
                string[] modelFile = System.IO.File.ReadAllLines(fileName);

                //parse mode, 0 = idle, 1 = vertex points, 2 = index connections, 3 = finished
                int parseMode = 0;
                //flag to ignore the current line or not
                bool ignoreLine = false;
                //list to contain vertex coordinates
                List<float> vertexValues = new List<float>();
                //list to contain the index order
                List<short> indexValues = new List<short>();
                List<float> tempValues = new List<float>();
                xToZ = false;

                #region Model file reading
                //loop through each line from the model file
                for (int i = 0; i < modelFile.Length; i++)
                {
                    switch (modelFile[i])
                    {
                        case "":
                        case "--------------------":
                            ignoreLine = true;
                            break;
                        //Allows the model file to have simple formatting for clarity
                        //  , a blank line or a row of 20 dashes will be ignored
                        case "V":
                            // V indicates the start of the vertex coord list
                            parseMode = 1;
                            ignoreLine = true;
                            break;
                        case "I":
                            // I switches to index connections
                            parseMode = 2;
                            ignoreLine = true;
                            break;
                        case "S":
                            // S indicates the model's individual statistic, used for car parts
                            parseMode = 3;
                            ignoreLine = true;
                            break;
                        case "D":
                            // D switches to dimensions
                            parseMode = 4;
                            ignoreLine = true;
                            break;
                        case "C":
                            // C switches to center point
                            parseMode = 5;
                            ignoreLine = true;
                            break;
                        case "E":
                            // E should be at the end of the file
                            parseMode = 10;
                            ignoreLine = true;
                            break;
                        case "X":
                            xToZ = true;
                            break;
                        case "M":
                            // M - Model Scale Offset
                            parseMode = 6;
                            ignoreLine = true;
                            break;
                        case "B":
                            // Body Only - Part Position Offsets
                            parseMode = 7;
                            ignoreLine = true;
                            break;
                        case "N":
                            // Model Name
                            parseMode = 8;
                            ignoreLine = true;
                            break;
                        default:
                            ignoreLine = false;
                            break;
                    }

                    if (!ignoreLine)
                    {
                        switch (parseMode)
                        {
                            //VERTEX VALUES
                            case 1:
                                vertexValues.Add(float.Parse(modelFile[i]));
                                break;

                            //INDEX VALUES
                            case 2:
                                indexValues.Add(short.Parse(modelFile[i]));
                                break;

                            //STATISTIC
                            case 3:
                                modelStat = float.Parse(modelFile[i]);
                                tempValues.Clear();
                                break;

                            //DIMENSIONS
                            case 4:
                                tempValues.Add(float.Parse(modelFile[i]));

                                if (tempValues.Count() >= 3)
                                    {
                                        float f1 = tempValues[0];
                                        float f2 = tempValues[1];
                                        float f3 = tempValues[2];

                                        if (xToZ)
                                        {
                                            dimensions = new Vector3(f3, f2, f1);
                                        }
                                        else
                                        {
                                            dimensions = new Vector3(f1, f2, f3);
                                        }

                                        tempValues.Clear();
                                        parseMode = 10;
                                    }

                                break;

                            //CENTER POINT
                            case 5:
                                tempValues.Add(float.Parse(modelFile[i]));

                                if (tempValues.Count() >= 3)
                                {
                                    centerPoint = new Vector3(tempValues[0], tempValues[1], tempValues[2]);
                                    tempValues.Clear();
                                }
                                //  Sets the center point's coordinates
                                break;

                            //MODEL SCALE
                            case 6:
                                modelScale = float.Parse(modelFile[i]);
                                break;

                            //PART OFFSETS
                            case 7:
                                tempValues.Add(float.Parse(modelFile[i]));
                                if (partOffsets.Count() < 3 && tempValues.Count >= 3)
                                {
                                    partOffsets.Add(new Vector3(tempValues[0], tempValues[1], tempValues[2]));
                                    tempValues.Clear();
                                }
                                else
                                {
                                }
                                // Populating partOffsets with values from the file in order.
                                // Main Booster -> Left Booster -> Front-Left Wheel
                                break;

                            case 8:
                                name = modelFile[i].ToString();
                                break;
                            //END
                            case 10:
                                //  Nothing here yet, can be used for debugging to test if
                                //  the function has reached the end of the file
                                break;
                            default:
                                break;
                        }
                    }
                }
                tempValues.Clear();
                #endregion

                #region Vertex Processing
                // Group the list of vertex points into Vector3 co-ordinates
                List<Vector3> VerticesList = new List<Vector3>();
                for (int i = 0; i < (vertexValues.Count); i+=3)
                {
                    float x = vertexValues[i];
                    float y = vertexValues[i + 1];
                    float z = vertexValues[i + 2];

                    //  Swap the x and z if swap flag is true
                    if (xToZ)
                    {
                        float oldX = x;
                        x = z;
                        z = oldX;
                    }

                    VerticesList.Add(new Vector3(x, y, z));
                }
                #endregion

                // Create the Model out of the vertices and indices
                BuildModelMesh(VerticesList, indexValues.ToArray());
            }

        }

        public override void update(float dt)
        {
            base.update(dt);
        }

        //public abstract void setRenderState(GraphicsDevice graphicsDevice, Camera cam);

        public void drawFillModel(GraphicsDevice graphicsDevice, Camera cam, int NumberOfPrimitives)
        {
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.SetVertexBuffer(myVBuffer);
            graphicsDevice.Indices = myIBuffer;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphicsDevice.RasterizerState = rasterizerState;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            basicEffect.Alpha = Opacity;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(ModelPrimitiveType, 0, 0,
                    myVBuffer.VertexCount, 0, NumberOfPrimitives);
            }
        }

        //  Same as draw but with wireframes instead of polygons
        public void drawWireFrameModel(GraphicsDevice graphicsDevice, Camera cam, int NumberOfPrimitives)
        {
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.SetVertexBuffer(myVWFBuffer);
            graphicsDevice.Indices = myIBuffer;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.WireFrame;
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.DepthBias = -0.000005f; ;
            graphicsDevice.RasterizerState = rasterizerState;
            graphicsDevice.BlendState = BlendState.Opaque;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(ModelPrimitiveType, 0, 0,
                    myVBuffer.VertexCount, 0, NumberOfPrimitives);
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            if (modelVertices != null)
            {
                basicEffect.World = worldMat;

                // Number of primitives per model
                int NumberOfPrimitives = 0;
                // Calcuate the number of triangles in the model
                switch (ModelPrimitiveType)
                {
                    case (PrimitiveType.TriangleList):
                        NumberOfPrimitives = myIBuffer.IndexCount / 3;
                        break;
                    case (PrimitiveType.TriangleStrip):
                        NumberOfPrimitives = myIBuffer.IndexCount - 2;
                        break;
                    case (PrimitiveType.LineList):
                        NumberOfPrimitives = myIBuffer.IndexCount / 3;
                        break;
                    case (PrimitiveType.LineStrip):
                        NumberOfPrimitives = myIBuffer.IndexCount - 2;
                        break;
                }

                basicEffect.Projection = cam.projMat;
                basicEffect.View = cam.viewMat;

                if (HasFillModel)
                {
                    drawFillModel(graphicsDevice, cam, NumberOfPrimitives);
                }

                if (HasWireFrameModel)
                {
                    drawWireFrameModel(graphicsDevice, cam, NumberOfPrimitives);
                }
            }
        }
    }
}
