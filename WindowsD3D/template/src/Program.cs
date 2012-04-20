using System;
using System.Windows.Forms;
using SlimDX.Windows;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace Template
{
    static class Program
    {

        static void PrintArgs(string[] args)
        {
            Console.WriteLine("Number of command line parameters = {0}",
            args.Length);
            foreach (string s in args)
            {
                Console.WriteLine(s);
            }

        }
        static void Main(string[] args)
        {

            Renderer myRenderer = new Renderer();
            var form = new RenderForm("Template");

            myRenderer.Init(form);

            // handle alt+enter ourselves
            form.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    form.Close();
                }

                if (e.Alt && e.KeyCode == Keys.Enter)
                {
                    myRenderer.ToggleFullScreen();
                }
            };

            // handle form size changes
            form.UserResized += (o, e) =>
            {
                myRenderer.Resize();
            };

            MessagePump.Run(form, () =>
            {
                myRenderer.Draw();
            });

            myRenderer.Release();
        }
    }
}
