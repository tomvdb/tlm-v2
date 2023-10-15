using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tlm_v2.TLMApp.Windows;
using ImGuiNET;
using ImPlotNET;
using System.Numerics;
using tlm_v2.Models;
using One_Sgp4;
using IconFonts;
using tlm_v2.Services;
using tlm_v2_common;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace tlm_v2.TLMApp
{
    internal class TLM
    {
        //private List<ReceivedPacket> packetsBuffer = new List<ReceivedPacket>();
        //private PluginLoader pluginManager = new PluginLoader();
        //private SimpleTcpClient? kissTcpClient;

        private DataInputKiss _dataInput;
        private List<double> _elevationBuffer = new List<double>();

        private KISSPlayer? _kissPlayer;

        private List<SatelliteInfo> satelliteList = new List<SatelliteInfo>();
        int selectedSatellite = -1;
        private ObserverInfo _observer;
        List<Pass> _futurePasses = new List<Pass>();

        // general decoding settings
        bool _filterOnVisibility = true;
        bool _forwardToSatNogs = false;
        bool _forwardDecodedFramesOnly = true;

        // windows
        DebugWindow _debugWindow = new DebugWindow();
        KISSViewerWindow _kISSViewerWindow = new KISSViewerWindow();

        FileOpenWindow _selectKissFile;

        int _appMode = 1;   // 0 = live, 1 = playback


        public void debug(string text)
        {
            _debugWindow.AddItem(text);
        }

        #region stylestesting

        public static void SetupImGuiStyle()
        {
            // Classic Steam stylemetasprite from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 0.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 0.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 0.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 0.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.1372549086809158f, 0.1568627506494522f, 0.1098039224743843f, 0.5199999809265137f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.2666666805744171f, 0.2980392277240753f, 0.2274509817361832f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.2980392277240753f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0f, 0.0f, 0.0f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.2784313857555389f, 0.3176470696926117f, 0.239215686917305f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.2470588237047195f, 0.2980392277240753f, 0.2196078449487686f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.2274509817361832f, 0.2666666805744171f, 0.2078431397676468f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.2862745225429535f, 0.3372549116611481f, 0.2588235437870026f, 0.4000000059604645f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 0.6000000238418579f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.5f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.1372549086809158f, 0.1568627506494522f, 0.1098039224743843f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1882352977991104f, 0.2274509817361832f, 0.1764705926179886f, 0.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.5372549295425415f, 0.5686274766921997f, 0.5098039507865906f, 0.7799999713897705f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.239215686917305f, 0.2666666805744171f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.3490196168422699f, 0.4196078479290009f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.6078431606292725f, 0.6078431606292725f, 0.6078431606292725f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.0f, 0.7764706015586853f, 0.2784313857555389f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.6000000238418579f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.2274509817361832f, 0.2274509817361832f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.729411780834198f, 0.6666666865348816f, 0.239215686917305f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.5882353186607361f, 0.5372549295425415f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.3499999940395355f);
        }
        public static void SetupImGuiStyle3()
        {
            // Visual Studio styleMomoDeve from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 0.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 0.0f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 0.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 0.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.5921568870544434f, 0.5921568870544434f, 0.5921568870544434f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.321568638086319f, 0.321568638086319f, 0.3333333432674408f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.3529411852359772f, 0.3529411852359772f, 0.3725490272045135f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.3529411852359772f, 0.3529411852359772f, 0.3725490272045135f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.3058823645114899f, 0.3058823645114899f, 0.3058823645114899f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2156862765550613f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.321568638086319f, 0.321568638086319f, 0.3333333432674408f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.1137254908680916f, 0.5921568870544434f, 0.9254902005195618f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.2274509817361832f, 0.2274509817361832f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.0f, 0.4666666686534882f, 0.7843137383460999f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1490196138620377f, 1.0f);
        }

        public static void SetupImGuiStyle1()
        {
            // Photoshop styleDerydoca from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new Vector2(8.0f, 8.0f);
            style.WindowRounding = 4.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 4.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 2.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(4.0f, 3.0f);
            style.FrameRounding = 4.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 13.0f;
            style.ScrollbarRounding = 12.0f;
            style.GrabMinSize = 7.0f;
            style.GrabRounding = 0.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 1.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1764705926179886f, 0.1764705926179886f, 0.1764705926179886f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.2784313857555389f, 0.2784313857555389f, 0.2784313857555389f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.2627451121807098f, 0.2627451121807098f, 0.2627451121807098f, 1.0f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.1568627506494522f, 0.1568627506494522f, 0.1568627506494522f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.2000000029802322f, 0.2000000029802322f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.2784313857555389f, 0.2784313857555389f, 0.2784313857555389f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.1921568661928177f, 0.1921568661928177f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.1568627506494522f, 0.1568627506494522f, 0.1568627506494522f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.2745098173618317f, 0.2745098173618317f, 0.2745098173618317f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.2980392277240753f, 0.2980392277240753f, 0.2980392277240753f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.3882353007793427f, 0.3882353007793427f, 0.3882353007793427f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(1.0f, 1.0f, 1.0f, 0.1560000032186508f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(1.0f, 1.0f, 1.0f, 0.3910000026226044f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.4666666686534882f, 0.4666666686534882f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.4666666686534882f, 0.4666666686534882f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.2627451121807098f, 0.2627451121807098f, 0.2627451121807098f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.3882353007793427f, 0.3882353007793427f, 0.3882353007793427f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.0f, 1.0f, 1.0f, 0.25f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1.0f, 1.0f, 1.0f, 0.6700000166893005f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.09411764889955521f, 0.09411764889955521f, 0.09411764889955521f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.3490196168422699f, 0.3490196168422699f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.1921568661928177f, 0.1921568661928177f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.09411764889955521f, 0.09411764889955521f, 0.09411764889955521f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.1921568661928177f, 0.1921568661928177f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.4666666686534882f, 0.4666666686534882f, 0.4666666686534882f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.5843137502670288f, 0.5843137502670288f, 0.5843137502670288f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.1882352977991104f, 0.1882352977991104f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.2274509817361832f, 0.2274509817361832f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(1.0f, 1.0f, 1.0f, 0.1560000032186508f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.0f, 0.3882353007793427f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.5860000252723694f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.0f, 0.0f, 0.0f, 0.5860000252723694f);
        }
        
        #endregion

        public TLM() 
        {
            debug("Starting...");

            debug(" - Load Satellite Data");
            string[] greencube_tle = new string[2] { "1 53106U 22080B   23284.00607016 -.00000003  00000-0  00000-0 0  9990", "2 53106  70.1389 279.4666 0008424 164.5168 195.5750  6.42557237 29198" };
            satelliteList.Add(new SatelliteInfo("Greencube", greencube_tle[0], greencube_tle[1]));

            debug(" - Load Observer Info");
            _observer = new ObserverInfo("ZR6TG", -26.709, 27.934, 1400);

            SetupImGuiStyle1();

            //_selectKissFile = new FileOpenWindow("Select File###FileExplorer", System.AppDomain.CurrentDomain.BaseDirectory);
            _selectKissFile = new FileOpenWindow("Select File###FileExplorer", Settings.KissPath); 
            _selectKissFile.OnNewDataPlayFile += _selectKissFile_OnNewDataPlayFile;
            _dataInput = new DataInputKiss(Settings.KissHost, Settings.KissPort);
            _dataInput.OnDataReceived += _dataInput_OnDataReceived;

            // load decoders
            SatDecoders.UpdateDecoders();
        }

        private void _selectKissFile_OnNewDataPlayFile(string FileName)
        {
            debug("New Data File");
            _kissPlayer = new KISSPlayer(FileName);
            debug(_kissPlayer.Count.ToString() + " Packets Ready");
        }

        private void _dataInput_OnDataReceived(byte[] data)
        {
            ReceiveKissData(new KISSPacket(data));
        }

        // this is where we process kiss data initially regardless from the source (live/playback)
        private void ReceiveKissData(KISSPacket data)
        {
            debug("Received Kiss Data:");
            debug(data.ToString());
            _kISSViewerWindow.AddItem(data);

            // send to active decoders
            SatDecoders.SubmitKissFrame(data);
        }

        public void TextCentered(string Text)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var textWidth = ImGui.CalcTextSize(Text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(Text);
        }

        public void TextCentered(string Text, Vector4 Color)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var textWidth = ImGui.CalcTextSize(Text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.TextColored(Color, Text);
        }


        public void RenderSatelliteInfo()
        {
            ImGui.SeparatorText(FontAwesome6.MapPin + " Satellite Position");
            ImGui.Dummy(new Vector2(0f, 5f));

            ImGui.BeginTable("Info", 2);
            TableLine("Az/El :", "AZ: " + satelliteList[selectedSatellite].Azimuth.ToString("0.##") + ",  EL: " + satelliteList[selectedSatellite].Elevation.ToString("0.##"));
            TableLine("Range (km):", satelliteList[selectedSatellite].Range.ToString());
            TableLine("Visible :", satelliteList[selectedSatellite].VisibleFromObserver.ToString());
            ImGui.EndTable();

        }

        public void TableLine(string property, string value)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(property);
            ImGui.TableNextColumn();
            ImGui.Text(value);
        }

        public void RenderObserverInfo()
        {
            ImGui.SeparatorText(FontAwesome6.House + " Observer");
            ImGui.Dummy(new Vector2(0f, 5f));

            ImGui.BeginTable("Info", 2);
            TableLine("Callsign/Name :", _observer.Callsign);
            TableLine("Location :", _observer.Latitude.ToString() + "," + _observer.Longitude.ToString() );
            TableLine("Altitude (m):", _observer.Height.ToString());

            ImGui.EndTable();
        }


        

        public void OpenKissFile(string FileName)
        {
            debug(FileName);

        }

        public void RenderFileExplorerWindow()
        {
            _selectKissFile.Render();
        }

        public double UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return timeSpan.TotalSeconds;
        }

        public void RenderFuturePasses()
        {
            ImGui.SeparatorText(FontAwesome6.Clock + " Future Passes");
            ImGui.Dummy(new Vector2(0f, 5f));
            // ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 16f);
            ImGui.BeginTable("Passes", 2, ImGuiTableFlags.BordersH | ImGuiTableFlags.PadOuterX);
            TableLine("Start", "Max Elevation");

            for (int x = 0; x < _futurePasses.Count; x++)
            {
                double maxEl = _futurePasses[x].getPassDetailOfMaxElevation().elevation;
                string passstart = _futurePasses[x].getPassDetailsAtStart().time.ToString();

                TableLine(passstart, maxEl.ToString("0.##"));
            }
            ImGui.EndTable();

            if (ImGui.Button(FontAwesome6.Recycle))
            {
                UpdateFuturePasses();
            }

        }

        private void SelectedSatelliteChanged()
        {
            UpdateFuturePasses();
        }

        private void UpdateFuturePasses()
        {
            if (selectedSatellite < 0) return;

            _futurePasses = satelliteList[selectedSatellite].GetFuturePasses(_observer);
        }

        public void RenderTrackingInfo()
        {
            if (ImGui.CollapsingHeader( FontAwesome6.SatelliteDish + " Tracking", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.SeparatorText( FontAwesome6.Satellite + " Satellite");

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);

                if (ImGui.BeginCombo("##SatelliteSelection", (selectedSatellite < 0) ? "Select Satellite..." : satelliteList[selectedSatellite].Name))
                {
                    for (int n = -1; n < satelliteList.Count; n++)
                    {
                        bool is_selected = false;

                        if (selectedSatellite == n)
                        {
                            is_selected = true;
                        }

                        if (n < 0)
                        {
                            continue;
                        }
                        else
                        {
                            if (ImGui.Selectable(satelliteList[n].Name, is_selected))
                            {
                                selectedSatellite = n;
                                SelectedSatelliteChanged();
                            }
                        }

                        if (is_selected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.Dummy(new Vector2(0, 20f));

                if (selectedSatellite >= 0)
                {
                    ImGui.Dummy(new Vector2(0f, 5f));
                    RenderObserverInfo();
                    ImGui.Dummy(new Vector2(0f, 5f));
                    RenderSatelliteInfo();
                    ImGui.Dummy(new Vector2(0f, 5f));
                    RenderFuturePasses();
                    ImGui.Dummy(new Vector2(0f, 5f));
                }
                else
                {
                    TextCentered("No Satellite Selected");
                }

                ImGui.Dummy(new Vector2(0, 20f));

            }
        }

        public void RenderSatelliteSettings()
        {
            if (ImGui.CollapsingHeader(FontAwesome6.Satellite + " TLE Settings"))
            {
                ImGui.Text("Satellite:");
            }
        }

        public void RenderDecoderSettings()
        {
            if (ImGui.CollapsingHeader(FontAwesome6.MagnifyingGlassChart +  " Decoder Settings"))
            {
                ImGui.SeparatorText("General");

                ImGui.Checkbox("Filter on Visibility", ref _filterOnVisibility);
                ImGui.Checkbox("Forward to SatNOGS", ref _forwardToSatNogs);
                ImGui.Checkbox("Forward Decoded Frames Only", ref _forwardDecodedFramesOnly);
            }

        }

        float time = 0;

        public void RenderStatusInfo()
        {
            if (ImGui.CollapsingHeader(FontAwesome6.Plug + " Connection Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Dummy(new Vector2(0f, 10f));

                if (_dataInput != null)
                {
                    if (!_dataInput.Connected)
                    {
                        TextCentered(FontAwesome6.PlugCircleExclamation + " KISS TNC Disconnected", new Vector4(1f, 0f, 0f, 0.8f));
                        ImGui.SameLine();
                        
                        if (ImGui.Button(FontAwesome6.Recycle))
                        {
                            // TODO: fire a seperate task to connect
                            //_dataInput.Connect();
                        }
                    }
                    else
                    {
                        TextCentered(FontAwesome6.PlugCircleCheck + " KISS TNC Connected", new Vector4(0f, 1f, 0f, 0.8f));
                    }
                }

                ImGui.Dummy(new Vector2(0f, 10f));

            }

        }

        public void RenderKissPlayer()
        {
            if (ImGui.CollapsingHeader(FontAwesome6.Play + " KISS Player", ImGuiTreeNodeFlags.DefaultOpen))
            {

                if (_kissPlayer == null)
                {
                    ImGui.Dummy(new Vector2(0f, 10f));

                    TextCentered("No KISS File Loaded");
                    ImGui.SameLine();
                    if (ImGui.Button(FontAwesome6.FolderOpen))
                    {
                        ImGui.OpenPopup("###FileExplorer");
                    }
                    ImGui.Dummy(new Vector2(0f, 10f));

                    RenderFileExplorerWindow();
                }
                else
                {
                    ImGui.Dummy(new Vector2(0f, 10f));
                    ImGui.TextWrapped(Path.GetFileName(_kissPlayer.FileName));
                    ImGui.Dummy(new Vector2(0f, 10f));
                    ImGui.Text(_kissPlayer.Current + "\\" + _kissPlayer.Count.ToString() + " Packets");
                    ImGui.Dummy(new Vector2(0f, 10f));

                    if (ImGui.Button("Get Next"))
                    {
                        var kissPacket = _kissPlayer.GetNext;

                        if (kissPacket != null)
                        {
                            ReceiveKissData(kissPacket);
                        }

                    }
                    ImGui.Dummy(new Vector2(0f, 10f));

                }

            }
        }


        public void RenderUI(int mainWindowWidth, int mainWindowHeight)
        {

            List<SatDecoder> toActivate = new List<SatDecoder>();
            List<SatDecoder> toDeactivate = new List<SatDecoder>();

            // TODO: move this into a timer
            if (selectedSatellite >= 0)
            {
                time += ImGui.GetIO().DeltaTime;

                if (time > 0.2)
                {
                    satelliteList[selectedSatellite].UpdateSatelliteData(_observer);
                    _elevationBuffer.Add(satelliteList[selectedSatellite].Elevation);

                    time = 0;
                }
            }


            int fixedBarSize = (int)(ImGui.GetFontSize() * 20);

            // render main toolbar fixed window
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(fixedBarSize, mainWindowHeight));
            ImGui.Begin("##toolbar", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking);

            if (ImGui.CollapsingHeader("TLM Mode", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Dummy(new Vector2(0f, 10f));

                if (ImGui.RadioButton("Live", _appMode == 0 ? true : false))
                {
                    _appMode = 0;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("Playback", _appMode == 1 ? true : false))
                {
                    if (_appMode == 0)
                    {
                        ImGui.OpenPopup("Confirmation");
                    }
                }
                ImGui.Dummy(new Vector2(0f, 10f));


                bool popen = true;
                if (ImGui.BeginPopupModal("Confirmation", ref popen, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("Are you sure you want to change app mode? Unsaved received data will be lost!");
                    
                    if (ImGui.Button("Yes"))
                    {
                        _appMode = 1;
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }

            }


            // render other
            if (_appMode == 0)
            {
                RenderStatusInfo();
                RenderTrackingInfo();
            }
            else
            {
                RenderKissPlayer();
            }

            if (ImGui.CollapsingHeader(FontAwesome6.ChartLine + " Decoders", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Dummy(new Vector2(0f, 5f));

                if (SatDecoders.Count == 0)
                {
                    TextCentered("No Decoders Available");
                }
                else
                {
                    SatDecoder[] allDecoders = SatDecoders.GetAll();


                    if (ImGui.BeginChild("##DecoderScrollingRegion", new Vector2(0, 10 * ImGui.GetFontSize()), false, ImGuiWindowFlags.HorizontalScrollbar))
                    {
                        if (ImGui.BeginTable("##DecoderTable", 2, ImGuiTableFlags.Sortable))
                        {
                            ImGui.TableSetupScrollFreeze(0, 1);

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.TableHeader("Active");
                            ImGui.TableSetColumnIndex(1);
                            ImGui.TableHeader("Decoder Name");


                            for (int i = 0; i < allDecoders.Length; i++)
                            {

                                SatDecoder decoder = allDecoders[i];

                                bool active = SatDecoders.isDecoderActive(decoder);

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                if (ImGui.Checkbox("##" + decoder.Name, ref active))
                                {
                                    //debug("Set Decoder Active:" + decoder.Name.ToString());
                                    if (active)
                                    {
                                        //SatDecoders.ActivateDecoder(decoder);
                                        toActivate.Add(decoder);
                                    }
                                    else
                                    {
                                        toDeactivate.Add(decoder);
                                        //SatDecoders.DeactivateDecoder(decoder); 
                                    }
                                }
                                ImGui.TableSetColumnIndex(1);
                                ImGui.Text(decoder.Name);
                            }

                            ImGui.EndTable();

                        }

                        ImGui.EndChild();
                    }
                }


                ImGui.Dummy(new Vector2(0f, 5f));

            }

            RenderDecoderSettings();
            RenderSatelliteSettings();

            ImGui.End();

            // render main toolbar fixed window
            ImGui.SetNextWindowPos(new Vector2(fixedBarSize, 0));
            ImGui.SetNextWindowSize(new Vector2(mainWindowWidth - fixedBarSize, mainWindowHeight));
            ImGui.Begin("##main", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus);
            ImGui.DockSpace(1);
            ImGui.End();

            /*
            ImPlot.ShowDemoWindow();
            ImGui.ShowDemoWindow();
            */

            SatDecoders.RenderActiveDecoders();

            // render misc windows
            _debugWindow.Render();
            _kISSViewerWindow.Render();


            // 
            for (int x = 0; x < toActivate.Count; x++)
            {
                SatDecoders.ActivateDecoder(toActivate[x]);
            }

            for (int x = 0; x < toDeactivate.Count; x++)
            {
                SatDecoders.DeactivateDecoder(toDeactivate[x]);
            }
        }

    }
}
