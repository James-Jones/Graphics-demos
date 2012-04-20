using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace Template
{
    class Renderer
    {
        Device device;
        SwapChain swapChain;
        ShaderSignature inputSignature;
        VertexShader vertexShader;
        PixelShader pixelShader;

        DataStream vertices;
        Buffer vertexBuffer;
        InputLayout layout;
        RenderTargetView renderTarget;
        DeviceContext context;

        public void Init(SlimDX.Windows.RenderForm form)
        {
            var description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            FeatureLevel[] flevel;

            flevel = new FeatureLevel[1];
            flevel[0] = FeatureLevel.Level_10_0;

            // DirectX DXGI 1.1 factory
            Factory1 factory1 = new Factory1();

            int adapterCount = factory1.GetAdapterCount();
            for (int i = 0; i < adapterCount; ++i)
            {
                // The 1st graphics adapter
                Adapter1 adapter1 = factory1.GetAdapter1(0);
            }


            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, flevel, description, out device, out swapChain);

            // create a view of our render target, which is the backbuffer of the swap chain we just created
            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                renderTarget = new RenderTargetView(device, resource);

            // setting a viewport is required if you want to actually see anything
            context = device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, form.ClientSize.Width, form.ClientSize.Height);
            context.OutputMerger.SetTargets(renderTarget);
            context.Rasterizer.SetViewports(viewport);

            // load and compile the vertex shader
            using (var bytecode = ShaderBytecode.CompileFromFile("shaders\\yellow.fx", "VShader", "vs_4_0", ShaderFlags.None, EffectFlags.None))
            {
                inputSignature = ShaderSignature.GetInputSignature(bytecode);
                vertexShader = new VertexShader(device, bytecode);
            }

            // load and compile the pixel shader
            using (var bytecode = ShaderBytecode.CompileFromFile("shaders\\yellow.fx", "PShader", "ps_4_0", ShaderFlags.None, EffectFlags.None))
                pixelShader = new PixelShader(device, bytecode);

            // create test vertex data, making sure to rewind the stream afterward
            vertices = new DataStream(12 * 3, true, true);
            vertices.Write(new Vector3(0.0f, 0.5f, 0.5f));
            vertices.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vertices.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vertices.Position = 0;

            // create the vertex layout and buffer
            var elements = new[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0) };
            layout = new InputLayout(device, inputSignature, elements);
            vertexBuffer = new Buffer(device, vertices, 12 * 3, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            // configure the Input Assembler portion of the pipeline with the vertex data
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 12, 0));

            // set the shaders
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            // prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
            using (var factory = swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        public void ToggleFullScreen()
        {
            swapChain.IsFullScreen = !swapChain.IsFullScreen;
        }

        public void Resize()
        {
            renderTarget.Dispose();

            swapChain.ResizeBuffers(2, 0, 0, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
                renderTarget = new RenderTargetView(device, resource);

            context.OutputMerger.SetTargets(renderTarget);
        }

        public void Draw()
        {
            // clear the render target to a soothing blue
            context.ClearRenderTargetView(renderTarget, new Color4(0.5f, 0.5f, 1.0f));

            // draw the triangle
            context.Draw(3, 0);
            swapChain.Present(0, PresentFlags.None);
        }

        public void Release()
        {
            // clean up all resources
            // anything we missed will show up in the debug output
            vertices.Close();
            vertexBuffer.Dispose();
            layout.Dispose();
            inputSignature.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            renderTarget.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }
    }
}
