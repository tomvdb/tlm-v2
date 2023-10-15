using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tlm_v2.Models;
using ImGuiNET;
using System.Collections;
using System.Linq;
using tlm_v2_common;
using tlm_v2_example_decoder_plugin;
using System.IO;
using System.Reflection;
using tlmv2_decoder_plugin_interface;

namespace tlm_v2.Services
{
    public static class SatDecoders
    {
        private static List<SatDecoder> _decoders = new List<SatDecoder>();
        private static List<SatDecoder> _activeDecoders = new List<SatDecoder>();

        public static int Count { get => _decoders.Count; }

        public static SatDecoder[] GetAll()
        {
            return _decoders.ToArray();
        }

        public static void RenderActiveDecoders()
        {
            for (int x = 0; x < _activeDecoders.Count; x++)
            {
                RenderDecoderWindow(_activeDecoders[x]);
            }
        }

        private static void RenderTelemetryContent(List<DecodedData> data)
        {

            if (data.Count == 0)
            {
                ImGui.Text("No Decoded Data");
                return;
            }

            // generate headers
            List<string> headers = new List<string>();

            foreach (string s in data[0].Data.Keys)
                headers.Add(s);

            if (ImGui.BeginChild("##ScrollingRegion", new System.Numerics.Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - (ImGui.GetFontSize() * 4)), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (ImGui.BeginTable("##Table", headers.Count, ImGuiTableFlags.Sortable | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY ))
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableNextRow();

                    // show headers
                    for (int h = 0; h < headers.Count; h++)
                    {
                        ImGui.TableSetColumnIndex(h);
                        ImGui.TableHeader(headers[h]);
                    }

                    for (int x = 0; x < data.Count; x++)
                    {
                        ImGui.TableNextRow();
                        for (int h = 0; h < headers.Count; h++)
                        {
                            ImGui.TableSetColumnIndex(h);
                            ImGui.Text(data[x].Data[headers[h]].ToString());
                        }
                    }

                    ImGui.EndTable();
                }


                ImGui.EndChild();
            }
        }

        private static void RenderGraphicsContent(List<DecodedData> data)
        {
            if (ImGui.BeginChild("##ScrollingRegion", new System.Numerics.Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - (ImGui.GetFontSize() * 4)), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                for (int x = 0; x < data.Count; ++x)
                {
                    ImGui.Text(data[x].Data["timestamp"].ToString());
                }
                ImGui.EndChild();
            }
        }

        public static void RenderDecoderWindow(SatDecoder Decoder)
        {

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Decoder: " + Decoder.Name))
            {
                ImGui.BeginTabBar("Test");

                for (int group = 0; group < Decoder.DataGroups.Count; group++)
                {
                    if (ImGui.BeginTabItem(Decoder.DataGroups[group].GroupName))
                    {
                        
                        switch (Decoder.DataGroups[group].GroupType)
                        {
                            case 0: // telemetry
                                var telemData = Decoder.DecodedData.Where(o => o.Type == 0).ToList();
                                RenderTelemetryContent(telemData);
                                break;
                            case 1: // graphics
                                var graphData = Decoder.DecodedData.Where(o => o.Type == 1).ToList();
                                RenderGraphicsContent(graphData);
                                break;

                            default: break;

                        }

                    

                        ImGui.EndTabItem();

                    }
                }

                ImGui.EndTabBar();
                ImGui.End();
            }

        }

        public static bool ActivateDecoder(SatDecoder decoder)
        {
            Console.WriteLine("Activate: " + decoder.Name.ToString());
            _activeDecoders.Add(decoder);
            decoder.Activate();
            return true;
        }

        public static bool DeactivateDecoder(SatDecoder decoder)
        {
            Console.WriteLine("DeActivate: " + decoder.Name.ToString());
            _activeDecoders.Remove(decoder);
            decoder.Deactivate();           
            return true;
        }

        public static bool isDecoderActive(SatDecoder decoder)
        {

            for (int c = 0; c <  _activeDecoders.Count; c++)
            {
                if (decoder == _activeDecoders[c])
                { 
                    return true; 
                }    
            }

            return false;

        }

        public static void SubmitKissFrame(KISSPacket packet)
        {
            for (int x = 0; x < _activeDecoders.Count; x++)
            {
                _activeDecoders[x].SubmitKissFrame(packet);
            }
        }

        public static int UpdateDecoders()
        {
            int count = 0;

            string pluginPath = System.AppDomain.CurrentDomain.BaseDirectory + "/plugins/";

            //Load the DLLs from the Plugins directory
            if (Directory.Exists(pluginPath))
            {
                string[] files = Directory.GetFiles(pluginPath);

                foreach (string file in files)
                {
                    Console.WriteLine(file);
                    if (file.EndsWith(".dll"))
                    {
                        Console.WriteLine("Found dll");
                        Console.WriteLine(file);
                        Assembly.LoadFile(Path.GetFullPath(file));
                    }
                }
            }

            try
            {
                Type interfaceType = typeof(IDecoderPlugin);
                Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass)
                    .ToArray();

                foreach (Type type in types)
                {
                    //Create a new instance of all found types                   
                    _decoders.Add(new SatDecoder((IDecoderPlugin)Activator.CreateInstance(type)));
                    count++;
                }
            }
            catch (Exception Ex)
            {
            }

            //_decoders.Add(new SatDecoder(new ExamplePlugin()));
            //count++;

            return count;
        }
    }
}
