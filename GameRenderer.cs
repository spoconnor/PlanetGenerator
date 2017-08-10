using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using Sean.Shared;
using OpenTK.Input;
using System.IO;
using OpenTK.Graphics;

namespace PlanetGenerator
{
	class GameRenderer : GameWindow
	{
        TextRenderer renderer;
		Landscape Landscape;
		MouseState mouseState;
		Font serif = new Font(FontFamily.GenericSerif, 24);
		Font sans = new Font(FontFamily.GenericSansSerif, 24);
		Font mono = new Font(FontFamily.GenericMonospace, 24);
		float angle;
		int[] textures = new int[255];
        int boxListIndex;
		float mousePosX, mousePosY;

		float Scale = 1.0f;
		int Direction = 1;
		Vector3 LookingAt;

		float lookingX=0.0f,lookingY=0.0f,lookingZ=0.0f;
		float cameraX=0.0f,cameraY=0.0f,cameraZ=5.0f;
		float rotationX = 0.0f, rotationY = 0.0f;

        Vector3d mouseSelect1 = new Vector3d(0, 0, 0);
        Vector3d mouseSelect2 = new Vector3d(0, 0, 0);

        public GameRenderer(Landscape landscape)
			:base(800,600, GraphicsMode.Default, "Planet Generator",
				GameWindowFlags.Default, DisplayDevice.Default,
				// ask for an OpenGL 3.0 forward compatible context
				3, 0, GraphicsContextFlags.ForwardCompatible)
		{
			this.Landscape = landscape;
			this.KeyPress += OnKeyPress;
			this.MouseWheel += OnMouseWheel;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
        }

        protected override void OnLoad(EventArgs e)
		{
			var black = new OpenTK.Vector4( 0.0f, 0.0f, 0.0f, 1.0f );
			var yellow = new OpenTK.Vector4( 1.0f, 1.0f, 0.0f, 1.0f );
			var cyan = new OpenTK.Vector4( 0.0f, 1.0f, 1.0f, 1.0f );
			var white = new OpenTK.Vector4( 1.0f, 1.0f, 1.0f, 1.0f );
			var direction = new OpenTK.Vector4( 1.0f, 1.0f, 1.0f, 0.0f );

			GL.Material (MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, cyan);
			GL.Material (MaterialFace.Front, MaterialParameter.Specular, white);
			GL.Material (MaterialFace.Front, MaterialParameter.Shininess, 30f);

			GL.Light (LightName.Light0, LightParameter.Ambient, black);
			GL.Light (LightName.Light0, LightParameter.Diffuse, yellow);
			GL.Light (LightName.Light0, LightParameter.Specular, white);
			GL.Light (LightName.Light0, LightParameter.Position, direction);

            //GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
            //GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
            //GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);

            GL.Enable(EnableCap.Lighting);                // so the renderer considers light
			GL.Enable(EnableCap.Light0);                  // turn LIGHT0 on
			GL.Enable(EnableCap.DepthTest);              // so the renderer considers depth

			// Set the current clear color to sky blue and the current drawing color to white.
			GL.ClearColor(0.1f, 0.39f, 0.88f, 1.0f);
			GL.Color3(1.0, 1.0, 1.0);

            float aspect_ratio = Width / (float)Height; // Aspect ratio of the screen
			float fov = MathHelper.PiOver2;  // camera field of view
			float near_distance = 0.1f; // The nearest the camera can see. >= 0.1f else clips
			float far_distance = 100.0f; // Fartherest the camera can see
    		OpenTK.Matrix4 perspective_matrix =
				OpenTK.Matrix4.CreatePerspectiveFieldOfView(fov, aspect_ratio, near_distance, far_distance);

			////Then we tell GL to use are matrix as the new Projection matrix.
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref perspective_matrix);

			GL.Enable(EnableCap.DepthTest);// 'Enable correct Z Drawings
			GL.DepthFunc(DepthFunction.Less);// 'Enable correct Z Drawings
			//GL.Viewport(0, 0, Width, Height);

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

            //GL.ShadeModel(ShadingModel.Smooth);  // TODO - requires vertex normals to be calulated across polys
        }

		private int LoadTexture(string filename)
		{
			int texture;
			Bitmap bitmap = new Bitmap(Path.Combine("Resources",filename));

			GL.GenTextures(1, out texture);
			GL.BindTexture(TextureTarget.Texture2D, texture);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                              ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
            bitmap.Dispose();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			return texture;
		}

		protected override void OnUnload(EventArgs e)
		{
			renderer.Dispose();
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(ClientRectangle);
		}

        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine($"Mouse click. {selectedBlock} - {selectedChunk} {selectedChunkHeight}");

            //GL.MatrixMode(MatrixMode.Projection);        // Select the Projection matrix for operation
            //GL.LoadIdentity();                           // Reset Projection matrix
            //GL.Ortho(0, this.Width * Global.Scale, 0, this.Height * Global.Scale, -1.0, 1.0);             // Set clipping area's left, right, bottom, top
            //GL.MatrixMode(MatrixMode.Modelview);         // Select the ModelView for operation
            //GL.LoadIdentity();                           // Reset the Model View Matrix

            //float pixels = 0.0f;
            //GL.ReadPixels<float>(mousePosX, mousePosY, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, ref pixels);
            //Console.WriteLine(pixels);

            mouseSelect1 = ScreenWorldConversion.ScreenToWorld(new Vector2d(e.X, e.Y));
            Console.WriteLine($"Mouse click:{mouseSelect1}");
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            //mousePosX = e.X * Global.Scale;
            //mousePosY = (this.Height - e.Y) * Global.Scale;
        }

        void OnKeyPress (object sender, KeyPressEventArgs e)
		{
            if (e.KeyChar == 's') {
                Landscape.SplitPolys();
            } else if (e.KeyChar == '+') {
				this.cameraZ -= (this.cameraZ / 10);
            } else if (e.KeyChar == '-') {
				this.cameraZ += (this.cameraZ / 10);
            } else if (e.KeyChar == 'p') {
				this.rotationY++;
            } else if (e.KeyChar == 'o') {
				this.rotationY--;
            } else if (e.KeyChar == 'q') {
				this.rotationX++;
            } else if (e.KeyChar == 'a') {
				this.rotationX--;
            } 
		}
			
		protected override void OnUpdateFrame(FrameEventArgs e)
		{			
			if (Keyboard[OpenTK.Input.Key.Escape])
			{
				this.Exit();
			}

			KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.W))
            {
            //    Global.LookingAt = new Position(Global.LookingAt.X - (int)Math.Max(1, Global.Scale), Global.LookingAt.Y, Global.LookingAt.Z);
            }

		}

      
        protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Matrix4 lookat = Matrix4.LookAt(0, 0, -7.5f + zoom, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview); // Swap to modelview so can draw the objects
            //GL.LoadMatrix(ref lookat);
			GL.LoadIdentity();

			GL.Translate(-cameraX, -cameraY, -cameraZ); // Translate back so can see the origin
			GL.Rotate (rotationX, 1f, 0f, 0f);
			GL.Rotate (rotationY, 0f, 1f, 0f);

            var d1 = new float[] { 0.2f, 0.5f, 0.8f, 1.0f };
            var d2 = new float[] { 0.3f, 0.8f, 0.4f, 1.0f };
            var d3 = new float[] { 0.7f, 0.2f, 0.2f, 1.0f };

            //GL.Enable(EnableCap.ColorMaterial);
            //GL.Color3(1, 0, 0);
            foreach (var poly in Landscape.GetPolys()) {

                // Color poly. Probably should be done in the shader
                if (poly.AverageVertex.Length > 1.0f)
                    GL.Material(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, d1);
                else
                    GL.Material(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, d3);

                GL.Begin (PrimitiveType.Triangles);

                //GL.Color3 (1, 1, 1); GL.Normal3(poly.Normal());
                //GL.Material(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, d1);
                GL.Vertex3 (poly.A.X, poly.A.Y, poly.A.Z);
				//GL.Color3 (1, 0, 0); GL.Normal3(poly.Normal());
                //GL.Material(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, d2);
                GL.Vertex3 (poly.B.X, poly.B.Y, poly.B.Z);
				//GL.Color3 (0, 1, 0); GL.Normal3(poly.Normal());
                //GL.Material(MaterialFace.Front, MaterialParameter.AmbientAndDiffuse, d3);
                GL.Vertex3 (poly.C.X, poly.C.Y, poly.C.Z);
				GL.End ();
			}
            //GL.Begin(PrimitiveType.Lines);
            //GL.Vertex3(mouseSelect1);
            //GL.Vertex3(0,0,0);
            //GL.End();

            SwapBuffers();

		/*
			foreach (var poly in Landscape.GetPolys())
            {
				//int texture = textures [(int)blockType];
				//if (texture == 0)
				//return;
            	GL.PushMatrix();
				{
					//GL.Translate (x, y, z);
					//GL.BindTexture (TextureTarget.Texture2D, texture);
					GL.Enable (EnableCap.Blend);
					GL.Enable (EnableCap.Texture2D);
					GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.Begin (PrimitiveType.Triangles);
					GL.TexCoord2 (0.0f, 1.0f);
					GL.Vertex3 (poly.A.X, poly.A.Y, poly.A.Z);
					GL.TexCoord2 (1.0f, 0.0f);
					GL.Vertex3 (poly.B.X, poly.B.Y, poly.B.Z);
					GL.TexCoord2 (0.0f, 0.0f);
					GL.Vertex3 (poly.C.X, poly.C.Y, poly.C.Z);
					GL.End ();
				}
            	GL.PopMatrix();
			}
			*/

		}

    }
}