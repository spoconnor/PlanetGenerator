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
        VertexBuffer<ColouredVertex> vertexBuffer;
        ShaderProgram shaderProgram;
        VertexArray<ColouredVertex> vertexArray;
        Matrix4Uniform projectionMatrix;

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
		float rotation;

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

			GL.Enable(EnableCap.Lighting);                // so the renderer considers light
			GL.Enable(EnableCap.Light0);                  // turn LIGHT0 on
			GL.Enable(EnableCap.DepthTest);              // so the renderer considers depth

			// Set the current clear color to sky blue and the current drawing color to white.
			GL.ClearColor(0.1f, 0.39f, 0.88f, 1.0f);
			GL.Color3(1.0, 1.0, 1.0);


            // create and fill a vertex buffer
            this.vertexBuffer = new VertexBuffer<ColouredVertex>(ColouredVertex.Size);

            this.vertexBuffer.AddVertex(new ColouredVertex(new Vector3(-1, -1, -1.5f), Color4.Lime));
            this.vertexBuffer.AddVertex(new ColouredVertex(new Vector3(1, 1, -1.5f), Color4.Red));
            this.vertexBuffer.AddVertex(new ColouredVertex(new Vector3(1, -1, -1.5f), Color4.Blue));

            // load shaders
            #region Shaders

            var vertexShader = new Shader(ShaderType.VertexShader,
                @"#version 130
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
// attributes of our vertex
in vec3 vPosition;
in vec4 vColor;
out vec4 fColor; // must match name in fragment shader
void main()
{
    // gl_Position is a special variable of OpenGL that must be set
	gl_Position = projectionMatrix * vec4(vPosition, 1.0);
	fColor = vColor;
}"
                );
            var fragmentShader = new Shader(ShaderType.FragmentShader,
                @"#version 130
in vec4 fColor; // must match name in vertex shader
out vec4 fragColor; // first out variable is automatically written to the screen
void main()
{
    fragColor = fColor;
}"
                );

            #endregion

            // link shaders into shader program
            this.shaderProgram = new ShaderProgram(vertexShader, fragmentShader);

            // create vertex array to specify vertex layout
            this.vertexArray = new VertexArray<ColouredVertex>(
                this.vertexBuffer, this.shaderProgram,
                new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, ColouredVertex.Size, 0),
                new VertexAttribute("vColor", 4, VertexAttribPointerType.Float, ColouredVertex.Size, 12)
            );

            float aspect_ratio = Width / (float)Height; // Aspect ratio of the screen

            // create projection matrix uniform
            this.projectionMatrix = new Matrix4Uniform("projectionMatrix");
            this.projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.PiOver2, aspect_ratio, 0.1f, 100f);

			//float fov = 1.0f;  // camera field of view
			//float near_distance = 1.0f; // The nearest the camera can see. >= 0.1f else clips
			//float far_distance = 1000.0f; // Fartherest the camera can see
    		//OpenTK.Matrix4 perspective_matrix =
			//	OpenTK.Matrix4.CreatePerspectiveFieldOfView(fov, (float)aspect_ratio, near_distance, far_distance);

			////Then we tell GL to use are matrix as the new Projection matrix.
			//GL.MatrixMode(MatrixMode.Projection);
			//GL.LoadMatrix(ref perspective_matrix);

			GL.Enable(EnableCap.DepthTest);// 'Enable correct Z Drawings
			GL.DepthFunc(DepthFunction.Less);// 'Enable correct Z Drawings
			//GL.Viewport(0, 0, Width, Height);

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
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

            //Console.WriteLine($"{mousePosX},{mousePosY}");
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            //mousePosX = e.X * Global.Scale;
            //mousePosY = (this.Height - e.Y) * Global.Scale;
        }

        void OnKeyPress (object sender, KeyPressEventArgs e)
		{
            if (e.KeyChar == '1') {
            //    Global.Direction = Facing.North;
            } else if (e.KeyChar == '2') {
            //    Global.Direction = Facing.East;
            } else if (e.KeyChar == '3') {
            //    Global.Direction = Facing.South;
            } else if (e.KeyChar == '4') {
            //    Global.Direction = Facing.West;
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

			GL.MatrixMode(MatrixMode.Modelview); // Swap to modelview so can draw the objects
			GL.LoadIdentity();

	//		GL.Translate(0, -1, -5); // Translate back so can see the origin
	//		GL.Rotate (rotation, 0f, 1f, 0f);



            // activate shader program and set uniforms
            this.shaderProgram.Use();
            this.projectionMatrix.Set(this.shaderProgram);

            // bind vertex buffer and array objects
            this.vertexBuffer.Bind();
            this.vertexArray.Bind();

            // upload vertices to GPU and draw them
            this.vertexBuffer.BufferData();
            this.vertexBuffer.Draw();

            // reset state for potential further draw calls (optional, but good practice)
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.UseProgram(0);



            foreach (var poly in Landscape.GetPolys()) {
				GL.Begin (PrimitiveType.Triangles);
				GL.Color3 (1, 1, 1); GL.Vertex3 (poly.A.X, poly.A.Y, poly.A.Z);
				GL.Color3 (1, 0, 0); GL.Vertex3 (poly.B.X, poly.B.Y, poly.B.Z);
				GL.Color3 (0, 1, 0); GL.Vertex3 (poly.C.X, poly.C.Y, poly.C.Z);
				GL.End ();
			}
            SwapBuffers();

			rotation++;

		/*
            //RenderGui();
            GL.MatrixMode(MatrixMode.Projection);        // Select the Projection matrix for operation
            GL.LoadIdentity();                           // Reset Projection matrix
			GL.Ortho(0, this.Width * Scale, 0, this.Height * Scale, -1.0, 1.0);             // Set clipping area's left, right, bottom, top
            GL.MatrixMode(MatrixMode.Modelview);         // Select the ModelView for operation
            GL.LoadIdentity();                           // Reset the Model View Matrix

            GL.Clear(ClearBufferMask.ColorBufferBit);// | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);

            GL.PushMatrix();
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
			GL.PopMatrix ();
            SwapBuffers();
			*/



		}

	}
}