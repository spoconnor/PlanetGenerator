using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using amulware.Graphics;

/*
namespace PlanetGenerator
{

	struct ColouredVertex
	{
		public const int Size = (3 + 4) * 4; // size of struct in bytes

		private readonly Vector3 position;
		private readonly Color4 color;

		public ColouredVertex(Vector3 position, Color4 color)
		{
			this.position = position;
			this.color = color;
		}
	}

	// when we pass our array of vertices to OpenGL, we will do so as a block of bytes, 
	// and it will not know how to interpret the data correctly.
	// To give it that information, specifically, to let it know what attributes our vertices have, 
	// and how they are laid out in memory, we will use a vertex array object (VAO).
	//
	// VAOs are very different from vertex buffers. Despite their name, they do not store vertices. 
	// Instead they store information about how to access an array of vertices, which is exactly what we want.
	sealed class VertexArray<TVertex>
		where TVertex : struct
	{
		private readonly int handle;

		public VertexArray(VertexBuffer<TVertex> vertexBuffer, ShaderProgram program,
			params VertexAttribute[] attributes)
		{
			// create new vertex array object
			GL.GenVertexArrays(1, out this.handle);

			// bind the object so we can modify it
			this.Bind();

			// bind the vertex buffer object
			vertexBuffer.Bind();

			// set all attributes
			foreach (var attribute in attributes)
				attribute.setAttribute(program);

			// unbind objects to reset state
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public void Bind()
		{
			// bind for usage (modification or rendering)
			GL.BindVertexArray(this.handle);
		}
	}
}

namespace Test
{
	// To render our vertices, we will need a vertex buffer object (VBO). You can think of this 
	// as a piece of GPU memory that we can copy our vertices to, so that they can be rendered.
	// Below you can see a very simple implementation wrapping all the functionality we will need.
	sealed class VertexBuffer<TVertex>
		where TVertex : struct // vertices must be structs so we can copy them to GPU memory easily
	{
		private readonly int vertexSize;
		private TVertex[] vertices = new TVertex[4];

		private int count;

		private readonly int handle;

		public VertexBuffer(int vertexSize)
		{
			this.vertexSize = vertexSize;

			// generate the actual Vertex Buffer Object
			this.handle = GL.GenBuffer();
		}

		public void AddVertex(TVertex v)
		{
			// resize array if too small
			if(this.count == this.vertices.Length)
				Array.Resize(ref this.vertices, this.count * 2);
			// add vertex
			this.vertices[count] = v;
			this.count++;
		}

		public void Bind()
		{
			// make this the active array buffer
			GL.BindBuffer(BufferTarget.ArrayBuffer, this.handle);
		}

		public void BufferData()
		{
			// copy contained vertices to GPU memory
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this.vertexSize * this.count),
				this.vertices, BufferUsageHint.StreamDraw);
		}

		public void Draw()
		{
			// draw buffered vertices as triangles
			GL.DrawArrays(PrimitiveType.Triangles, 0, this.count);
		}
	}

	sealed class VertexAttribute
	{
		private readonly string name;
		private readonly int size;
		private readonly VertexAttribPointerType type;
		private readonly bool normalize;
		private readonly int stride;
		private readonly int offset;

		public VertexAttribute(string name, int size, VertexAttribPointerType type,
			int stride, int offset, bool normalize = false)
		{
			this.name = name;
			this.size = size;
			this.type = type;
			this.stride = stride;
			this.offset = offset;
			this.normalize = normalize;
		}

		public void Set(ShaderProgram program)
		{
			// get location of attribute from shader program
			int index = program.GetAttributeLocation(this.name);

			// enable and set attribute
			GL.EnableVertexAttribArray(index);
			GL.VertexAttribPointer(index, this.size, this.type,
				this.normalize, this.stride, this.offset);
		}
	}

	sealed class Shader
	{
		private readonly int handle;

		public int Handle { get { return this.handle; } }

		public Shader(ShaderType type, string code)
		{
			// create shader object
			this.handle = GL.CreateShader(type);

			// set source and compile shader
			GL.ShaderSource(this.handle, code);
			GL.CompileShader(this.handle);
		}
	}

	sealed class ShaderProgram
	{
		private readonly int handle;

		public ShaderProgram(params Shader[] shaders)
		{
			// create program object
			this.handle = GL.CreateProgram();

			// assign all shaders
			foreach (var shader in shaders)
				GL.AttachShader(this.handle, shader.Handle);

			// link program (effectively compiles it)
			GL.LinkProgram(this.handle);

			// detach shaders
			foreach (var shader in shaders)
				GL.DetachShader(this.handle, shader.Handle);
		}

		public void Use()
		{
			// activate this program to be used
			GL.UseProgram(this.handle);
		}

		public int GetAttributeLocation(string name)
		{
			// get the location of a vertex attribute
			return GL.GetAttribLocation(this.handle, name);
		}

		public int GetUniformLocation(string name)
		{
			// get the location of a uniform variable
			return GL.GetUniformLocation(this.handle, name);
		}
	}

	sealed class Matrix4Uniform
	{
		private readonly string name;
		private Matrix4 matrix;

		public Matrix4 Matrix { get { return this.matrix; } set { this.matrix = value; } }

		public Matrix4Uniform(string name)
		{
			this.name = name;
		}

		public void Set(ShaderProgram program)
		{
			// get uniform location
			var i = program.GetUniformLocation(this.name);

			// set uniform value
			GL.UniformMatrix4(i, false, ref this.matrix);
		}
	}

	sealed class TestGameWindow : OpenTK.GameWindow
	{
		private VertexBuffer<ColouredVertex> vertexBuffer;
		private ShaderProgram shaderProgram;
		private VertexArray<ColouredVertex> vertexArray;
		private Matrix4Uniform projectionMatrix;

		public TestGameWindow()
		// set window resolution, title, and default behaviour
			:base(1280, 720, GraphicsMode.Default, "OpenTK Intro",
				GameWindowFlags.Default, DisplayDevice.Default,
				// ask for an OpenGL 3.0 forward compatible context
				3, 0, GraphicsContextFlags.ForwardCompatible)
		{
			Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, this.Width, this.Height);
		}

		protected override void OnLoad(EventArgs e)
		{
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

			// create projection matrix uniform
			this.projectionMatrix = new Matrix4Uniform("projectionMatrix");
			this.projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(
				MathHelper.PiOver2, 16f / 9, 0.1f, 100f);

		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			// this is called every frame, put game logic here
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			// clea the screen
			GL.ClearColor(Color4.Purple);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

			// swap backbuffer
			this.SwapBuffers();
		}
	}
}
*/