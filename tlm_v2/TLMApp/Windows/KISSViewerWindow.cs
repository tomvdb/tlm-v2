using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconFonts;
using ImGuiNET;
using tlm_v2_common;

namespace tlm_v2.TLMApp.Windows
{
    internal class KISSViewerWindow
    {
        List<KISSPacket> items = new List<KISSPacket>();
        bool scrollToBottom = false;
        bool cleanData = false;

        public KISSViewerWindow()
        {
        }

        public void Clear()
        {
            items.Clear();
        }

        public void AddItem(KISSPacket item)
        {
            items.Add(item);
            scrollToBottom = true;
        }

        public void Render()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.FirstUseEver);

            if (!ImGui.Begin(FontAwesome6.Frog + " KISS Packets"))
            {
                ImGui.End();
                return;
            }

            ImGui.Text("Received Packets: " + items.Count.ToString() + "");
            ImGui.Checkbox("Strip Kiss", ref cleanData);

            ImGui.Separator();

            if (ImGui.BeginChild("ScrollingRegion", new System.Numerics.Vector2(0, -10), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.PushFont(GlobalFonts.fixedFont);

                for (int c = 0; c < items.Count; c++)
                {

                    KISSPacket kISSPacket = items[c];

                    int counter = 0;
                    int memCount = 0;
                    int breakCount = 10;

                    ImGui.NewLine();
                    ImGui.SeparatorText("#" + (c + 1).ToString() + " - " + kISSPacket.TimeStamp.ToString() + " - " + kISSPacket.GetData(cleanData).Length.ToString());
                    ImGui.Text(memCount.ToString("X").PadLeft(4, '0') + " : ");

                    string ASCII = "";

                    for (int x = 0; x < kISSPacket.GetData(cleanData).Length; x++)
                    {
                        ImGui.SameLine();

                        bool highlight = false;
                        
                        if (cleanData == false)
                        {
                            if (x < 2) { highlight = true; }

                            switch (kISSPacket.GetData(cleanData)[x])
                            {
                                case 0xC0: 
                                case 0xDB: 
                                case 0xDD:
                                case 0xDC: highlight = true; break;
                            }
                        }


                        if (highlight)
                        {
                            ImGui.TextColored(new System.Numerics.Vector4(0f, 1f, 1f, 0.8f), kISSPacket.GetData(cleanData)[x].ToString("X").PadLeft(2, '0'));
                        }
                        else
                        {
                            ImGui.TextColored(new System.Numerics.Vector4(0f, 1f, 0f, 0.8f), kISSPacket.GetData(cleanData)[x].ToString("X").PadLeft(2, '0'));
                        }


                        if (kISSPacket.GetData(cleanData)[x] >= 32 && kISSPacket.GetData(cleanData)[x] >= 126)
                            ASCII += Convert.ToChar(kISSPacket.GetData(cleanData)[x]);
                        else
                            ASCII += ".";

                        counter++;

                        if (counter > breakCount)
                        {
                            ImGui.SameLine();
                            ImGui.SetCursorPosX(25 * ImGui.CalcTextSize("00").X);
                            ImGui.Text(ASCII);

                            memCount += 10;
                            counter = 0;
                            ASCII = "";

                            if (x+1 < kISSPacket.GetData(cleanData).Length)
                                ImGui.Text(memCount.ToString("X").PadLeft(4, '0') + " : ");
                        }
                    }

                    if (ASCII.Length  > 0)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(25 * ImGui.CalcTextSize("00").X);
                        ImGui.Text(ASCII);
                    }

                    // auto scroll
                    if (scrollToBottom && c == items.Count - 1)
                    {
                        ImGui.SetScrollHereY(1.0f);
                        scrollToBottom = false;
                    }


                    ImGui.Separator();

                }

                ImGui.PopFont();

            }

            ImGui.EndChild();
            ImGui.Separator();
            ImGui.End();

        }
    }
}
