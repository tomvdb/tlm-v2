using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using IconFonts;

namespace tlm_v2.TLMApp.Windows
{
    public class DebugWindow
    {
        List<string> items;
        bool scrollToBottom = false;
        bool autoScroll = true;

        public DebugWindow()
        {
            items = new List<string>();
        }

        public void Clear()
        {
            items.Clear();
        }

        public void AddItem(string item)
        {
            items.Add(item);
            scrollToBottom = true;
        }

        public void Render()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.FirstUseEver);

            if (!ImGui.Begin(FontAwesome6.Bug + " Debug"))
            {
                ImGui.End();
                return;
            }


            ImGui.SameLine();
            if (ImGui.Button(FontAwesome6.FloppyDisk + " Save"))
            {
                AddItem("Saving!");
            }
            ImGui.SameLine();
            if (ImGui.Button(FontAwesome6.TrashCan + " Clear"))
            {
                AddItem("Clear!");
            }
            ImGui.SameLine();
            ImGui.Checkbox("Auto Scroll", ref autoScroll);


            ImGui.Separator();

            if (ImGui.BeginChild("ScrollingRegion", new System.Numerics.Vector2(0, -10), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                for (int c = 0; c < items.Count; c++)
                {
                    ImGui.TextUnformatted(items[c]);

                    // auto scroll
                    if (autoScroll)
                    {
                        if (scrollToBottom && c == items.Count - 1)
                        {
                            ImGui.SetScrollHereY(1.0f);
                            scrollToBottom = false;
                        }
                    }
                }

            }

            ImGui.EndChild();
            ImGui.Separator();
            ImGui.End();

        }


    }
}
