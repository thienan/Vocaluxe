﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Vocaluxe.Base;
using Vocaluxe.Lib.Sound;
using Vocaluxe.Menu;


namespace Vocaluxe.Screens
{
    struct DelayTest
    {
        public Stopwatch Timer;
        public float Delay;
    }

    class CScreenOptionsRecord : CMenu
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        protected override int _ScreenVersion { get { return 2; } }

        private const float MaxDelayTime = 1f;
        private const string SelectSlideRecordDevices = "SelectSlideRecordDevices";
        private const string SelectSlideRecordInputs = "SelectSlideRecordInputs";

        private const string SelectSlideRecordChannel1 = "SelectSlideRecordChannel1";
        private const string SelectSlideRecordChannel2 = "SelectSlideRecordChannel2";
        private const string SelectSlideDelay = "SelectSlideDelay";

        private const string StaticWarning = "StaticWarning";
        private const string TextWarning = "TextWarning";

        private const string ButtonExit = "ButtonExit";
        private const string ButtonDelayTest = "ButtonDelayTest";

        private const string TextDelayChannel1 = "TextDelayChannel1";
        private const string TextDelayChannel2 = "TextDelayChannel2";

        private const string EqualizerChannel1 = "EqualizerChannel1";
        private const string EqualizerChannel2 = "EqualizerChannel2";

        private readonly string[] StaticEnergyChannel = new string[] { "StaticEnergyChannel1", "StaticEnergyChannel2" };
        private float[] ChannelEnergy;

        private SRecordDevice[] _devices;
        private int _DeviceNr;
        private int _InputNr;

        private DelayTest[] _DelayTest;
        private bool _DelayTestRunning;
        private int _DelaySound;

        public CScreenOptionsRecord()
        {
        }

        public override void Init()
        {
            base.Init();

            List<string> values = new List<string>();
            values.Add(StaticWarning);
            values.AddRange(StaticEnergyChannel);
            _ThemeStatics = values.ToArray();

            _ThemeTexts = new string[] { TextWarning, TextDelayChannel1, TextDelayChannel2 };
            _ThemeButtons = new string[] { ButtonExit, ButtonDelayTest };
            _ThemeSelectSlides = new string[] { SelectSlideRecordDevices, SelectSlideRecordInputs, SelectSlideRecordChannel1, SelectSlideRecordChannel2, SelectSlideDelay };
            _ThemeEqualizers = new string[] { EqualizerChannel1, EqualizerChannel2 };
        }

        public override void LoadTheme(string XmlPath)
        {
            base.LoadTheme(XmlPath);

            int max = CSettings.MaxNumPlayer + 1;
            if (max > 7)
                max = 7;

            SelectSlides[SelectSlideRecordChannel1].NumVisible = max;
            SelectSlides[SelectSlideRecordChannel2].NumVisible = max;

            SelectSlides[SelectSlideRecordChannel1].AddValue(CLanguage.Translate("TR_CONFIG_OFF"));
            SelectSlides[SelectSlideRecordChannel2].AddValue(CLanguage.Translate("TR_CONFIG_OFF"));

            for (int i = 1; i <= CSettings.MaxNumPlayer; i++)
            {
                SelectSlides[SelectSlideRecordChannel1].AddValue(i.ToString());
                SelectSlides[SelectSlideRecordChannel2].AddValue(i.ToString());
            }

            for (int i = 0; i < 26; i++)
            {
                SelectSlides[SelectSlideDelay].AddValue((i * 20).ToString() + " ms");
            }

            ChannelEnergy = new float[StaticEnergyChannel.Length];

            for (int i = 0; i < ChannelEnergy.Length; i++)
            {
                Statics[StaticEnergyChannel[i]].Visible = false;
                ChannelEnergy[i] = 0f;
            }

            Equalizers[EqualizerChannel1].ScreenHandles = true;
            Equalizers[EqualizerChannel2].ScreenHandles = true;
        }

        public override bool HandleInput(KeyEvent KeyEvent)
        {
            base.HandleInput(KeyEvent);

            if (KeyEvent.KeyPressed)
            {

            }
            else
            {
                switch (KeyEvent.Key)
                {
                    case Keys.Escape:
                    case Keys.Back:
                        SaveMicConfig();
                        CGraphics.FadeTo(EScreens.ScreenOptions);
                        break;

                    case Keys.S:
                        CParty.SetNormalGameMode();
                        CGraphics.FadeTo(EScreens.ScreenSong);
                        break;

                    case Keys.Enter:
                        if (Buttons[ButtonExit].Selected)
                        {
                            SaveMicConfig();
                            CGraphics.FadeTo(EScreens.ScreenOptions);
                        }
                        
                        if (Buttons[ButtonDelayTest].Selected)
                        {
                            TestDelay();
                        }
                             
                        break;

                    case Keys.D:
                        TestDelay();
                        break;

                    case Keys.Left:
                        if (SelectSlides[SelectSlideRecordDevices].Selected)
                            OnDeviceEvent();

                        if (SelectSlides[SelectSlideRecordInputs].Selected)
                            OnInputEvent();

                        if (SelectSlides[SelectSlideRecordChannel1].Selected ||
                            SelectSlides[SelectSlideRecordChannel2].Selected)
                            SetMicConfig();

                        if (SelectSlides[SelectSlideDelay].Selected)
                            SaveDelayConfig();
                        break;

                    case Keys.Right:
                        if (SelectSlides[SelectSlideRecordDevices].Selected)
                            OnDeviceEvent();

                        if (SelectSlides[SelectSlideRecordInputs].Selected)
                            OnInputEvent();

                        if (SelectSlides[SelectSlideRecordChannel1].Selected ||
                            SelectSlides[SelectSlideRecordChannel2].Selected)
                            SetMicConfig();

                        if (SelectSlides[SelectSlideDelay].Selected)
                            SaveDelayConfig();
                        break;
                }
            }
            return true;
        }

        public override bool HandleMouse(MouseEvent MouseEvent)
        {
            base.HandleMouse(MouseEvent);

            if (MouseEvent.RB)
            {
                SaveMicConfig();
                CGraphics.FadeTo(EScreens.ScreenOptions);
            }

            if (MouseEvent.LB && IsMouseOver(MouseEvent))
            {
                if (SelectSlides[SelectSlideRecordDevices].Selected)
                    OnDeviceEvent();

                if (SelectSlides[SelectSlideRecordInputs].Selected)
                    OnInputEvent();

                if (SelectSlides[SelectSlideRecordChannel1].Selected ||
                    SelectSlides[SelectSlideRecordChannel2].Selected)
                    SetMicConfig();

                if (SelectSlides[SelectSlideDelay].Selected)
                    SaveDelayConfig();

                if (Buttons[ButtonExit].Selected)
                {
                    SaveMicConfig();
                    CGraphics.FadeTo(EScreens.ScreenOptions);
                }

                if (Buttons[ButtonDelayTest].Selected)
                {
                    TestDelay();
                }
            }
            return true;
        }

        public override bool UpdateGame()
        {
            for (int i = 0; i < CSettings.MaxNumPlayer; i++)
            {
                CSound.AnalyzeBuffer(i);
            }

            if (_DelayTest != null)
            {
                for (int i = 0; i < _DelayTest.Length - 1; i++)
                {
                    if (_DelayTestRunning && !_DelayTest[i].Timer.IsRunning)
                    {
                        if (CSound.GetPosition(_DelaySound) > 0f)
                        {
                            _DelayTest[i].Timer.Reset();
                            _DelayTest[i].Timer.Start();
                        }
                    }
                    if (_DelayTest[i].Timer.IsRunning)
                    {
                        int player = 0;
                        if (i == 0)
                            player = SelectSlides[SelectSlideRecordChannel1].Selection;

                        if (i == 1)
                            player = SelectSlides[SelectSlideRecordChannel2].Selection;

                        if (_DelayTest[i].Timer.ElapsedMilliseconds > MaxDelayTime * 1000f || player == 0)
                        {
                            _DelayTest[i].Delay = 0f;
                            _DelayTest[i].Timer.Stop();
                            _DelayTestRunning = false;
                        }
                        else if (CSound.RecordGetMaxVolume(player - 1) > 0.1f &&
                            (CSound.RecordGetToneAbs(player - 1) == 9 || CSound.RecordGetToneAbs(player - 1) == 21 || CSound.RecordGetToneAbs(player - 1) == 33))
                        {
                            _DelayTest[i].Delay = _DelayTest[i].Timer.ElapsedMilliseconds;
                            _DelayTest[i].Timer.Stop();
                            _DelayTestRunning = false;
                        }
                    }
                }
                Texts[TextDelayChannel1].Text = _DelayTest[0].Delay.ToString("000") + " ms";
                Texts[TextDelayChannel2].Text = _DelayTest[1].Delay.ToString("000") + " ms";
            }


            if (CheckMicConfig())
            {
                ChannelEnergy[0] = 0f;
                int player = SelectSlides[SelectSlideRecordChannel1].Selection;
                if (player > 0)
                {
                    ChannelEnergy[0] = CSound.RecordGetMaxVolume(player - 1);
                    Equalizers[EqualizerChannel1].Update(CSound.ToneWeigth(player - 1));
                }
                else
                    Equalizers[EqualizerChannel1].Reset();

                ChannelEnergy[1] = 0f;
                player = SelectSlides[SelectSlideRecordChannel2].Selection;
                if (player > 0)
                {
                    ChannelEnergy[1] = CSound.RecordGetMaxVolume(player - 1);
                    Equalizers[EqualizerChannel2].Update(CSound.ToneWeigth(player - 1));
                }
                else
                    Equalizers[EqualizerChannel2].Reset();
            }
            else
            {
                for (int i = 0; i < ChannelEnergy.Length; i++)
                {
                    ChannelEnergy[i] = 0f;
                }
                Equalizers[EqualizerChannel1].Reset();
                Equalizers[EqualizerChannel2].Reset();
            }

            return true;
        }

        public override void OnShow()
        {
            base.OnShow();

            SelectSlides[SelectSlideRecordDevices].Clear();
            SelectSlides[SelectSlideRecordInputs].Clear();

            for (int i = 0; i < ChannelEnergy.Length; i++)
            {
                ChannelEnergy[i] = 0f;
            }

            _DeviceNr = -1;
            _InputNr = -1;

            _devices = CSound.RecordGetDevices();
            if (_devices != null)
            {
                _DeviceNr = 0;
                _InputNr = 0;
                GetFirstConfiguredRecordDevice(ref _DeviceNr, ref _InputNr);

                for (int dev = 0; dev < _devices.Length; dev++)
                {
                    SelectSlides[SelectSlideRecordDevices].AddValue(_devices[dev].Name);
                }
                SelectSlides[SelectSlideRecordDevices].Selection = _DeviceNr;

                for (int inp = 0; inp < _devices[0].Inputs.Count; inp++)
                {
                    SelectSlides[SelectSlideRecordInputs].AddValue(_devices[0].Inputs[inp].Name);
                }
                SelectSlides[SelectSlideRecordInputs].Selection = _InputNr;
                UpdateChannels();
            }

            SelectSlides[SelectSlideRecordChannel1].Visible = (_devices != null);
            SelectSlides[SelectSlideRecordChannel2].Visible = (_devices != null);
            
            Statics[StaticWarning].Visible = false;
            Texts[TextWarning].Visible = false;

            _DelayTest = null;
            if (_devices != null)
            {
                _DelayTest = new DelayTest[2];
                for (int i = 0; i < _DelayTest.Length - 1; i++)
                {
                    _DelayTest[i].Timer = new Stopwatch();
                    _DelayTest[i].Delay = 0f;
                }
            }

            SelectSlides[SelectSlideDelay].Selection = (int)(CConfig.MicDelay / 20);

            _DelayTestRunning = false;
            _DelaySound = -1;
        }

        public override void OnShowFinish()
        {
            base.OnShowFinish();
            CBackgroundMusic.Disabled = true;
        }

        public override bool Draw()
        {
            DrawBG();

            if (!CheckMicConfig())
            {
                Statics[StaticWarning].Visible = true;
                Texts[TextWarning].Visible = true;
            }
            else
            {
                Statics[StaticWarning].Visible = false;
                Texts[TextWarning].Visible = false;
            }

            for (int i = 0; i < StaticEnergyChannel.Length; i++)
            {
                if (ChannelEnergy[i] > 0f)
                {
                    SRectF rect = new SRectF(Statics[StaticEnergyChannel[i]].Rect.X,
                        Statics[StaticEnergyChannel[i]].Rect.Y,
                        Statics[StaticEnergyChannel[i]].Rect.W * ChannelEnergy[i],
                        Statics[StaticEnergyChannel[i]].Rect.H,
                        Statics[StaticEnergyChannel[i]].Rect.Z);

                    CDraw.DrawTexture(Statics[StaticEnergyChannel[i]].Texture, Statics[StaticEnergyChannel[i]].Rect,
                        new SColorF(1f, 1f, 1f, 1f), rect);
                }
            }

            Equalizers[EqualizerChannel1].Draw();
            Equalizers[EqualizerChannel2].Draw();

            DrawFG();

            return true;
        }

        public override void OnClose()
        {
            base.OnClose();
            CSound.RecordStop();

            _DelayTest = null;
            CBackgroundMusic.Disabled = false;
        }

        private void OnDeviceEvent()
        {
            if (SelectSlides[SelectSlideRecordDevices].Selection != _DeviceNr)
            {
                SelectSlides[SelectSlideRecordInputs].Clear();
                _DeviceNr = SelectSlides[SelectSlideRecordDevices].Selection;
                _InputNr = 0;

                for (int inp = 0; inp < _devices[_DeviceNr].Inputs.Count; inp++)
                {
                    SelectSlides[SelectSlideRecordInputs].AddValue(_devices[_DeviceNr].Inputs[inp].Name);
                }
                _InputNr = 0;
                SelectSlides[SelectSlideRecordInputs].Selection = 0;
                UpdateChannels();
            }
        }

        private void OnInputEvent()
        {
            if (SelectSlides[SelectSlideRecordInputs].Selection != _InputNr)
            {
                _InputNr = SelectSlides[SelectSlideRecordInputs].Selection;

                UpdateChannels();                
            }
        }

        private void SaveDelayConfig()
        {
            CConfig.MicDelay = SelectSlides[SelectSlideDelay].Selection * 20;
            CConfig.SaveConfig();
        }
        
        private void SaveMicConfig()
        {
			if (_devices == null)
				return;
			
            CSound.RecordStop();
            SetMicConfig();

            if (CheckMicConfig())
            {
                for (int p = 0; p < CSettings.MaxNumPlayer; p++)
                {
                    CConfig.MicConfig[p].Channel = 0;
                }

                for (int dev = 0; dev < _devices.Length; dev++)
                {
                    for (int inp = 0; inp < _devices[dev].Inputs.Count; inp++)
                    {
                        if (_devices[dev].Inputs[inp].PlayerChannel1 > 0)
                        {
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel1 - 1].Channel = 1;
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel1 - 1].DeviceName = _devices[dev].Name;
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel1 - 1].DeviceDriver = _devices[dev].Driver;
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel1 - 1].InputName = _devices[dev].Inputs[inp].Name;
                        }

                        if (_devices[dev].Inputs[inp].PlayerChannel2 > 0)
                        {
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel2 - 1].Channel = 2;
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel2 - 1].DeviceName = _devices[dev].Name;
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel2 - 1].DeviceDriver = _devices[dev].Driver;
                            CConfig.MicConfig[_devices[dev].Inputs[inp].PlayerChannel2 - 1].InputName = _devices[dev].Inputs[inp].Name;
                        }

                    }
                }
                CConfig.SaveConfig();
            }
            CSound.RecordStart();
        }

        private void SetMicConfig()
        {
            if (_DeviceNr < 0)
                return;
            SInput input = _devices[_DeviceNr].Inputs[_InputNr];
            input.PlayerChannel1 = SelectSlides[SelectSlideRecordChannel1].Selection;
            input.PlayerChannel2 = SelectSlides[SelectSlideRecordChannel2].Selection;
            _devices[_DeviceNr].Inputs[_InputNr] = input;
        }

        private void UpdateChannels()
        {
            SelectSlides[SelectSlideRecordChannel1].Selection = _devices[_DeviceNr].Inputs[_InputNr].PlayerChannel1;
            SelectSlides[SelectSlideRecordChannel2].Selection = _devices[_DeviceNr].Inputs[_InputNr].PlayerChannel2;

            SaveMicConfig();
        }

        private bool CheckMicConfig()
        {
            bool[] IsSet = new bool[CSettings.MaxNumPlayer];
            for (int i = 0; i < CSettings.MaxNumPlayer; i++)
			{
			    IsSet[i] = false;
			}

            if (_devices == null)
				return true;
			
            for (int dev = 0; dev < _devices.Length; dev++)
            {
                for (int inp = 0; inp < _devices[dev].Inputs.Count; inp++)
                {
                    if (_devices[dev].Inputs[inp].PlayerChannel1 > 0)
                    {
                        if (IsSet[_devices[dev].Inputs[inp].PlayerChannel1 - 1])
                            return false;

                        IsSet[_devices[dev].Inputs[inp].PlayerChannel1 - 1] = true;
                    }

                    if (_devices[dev].Inputs[inp].PlayerChannel2 > 0)
                    {
                        if (IsSet[_devices[dev].Inputs[inp].PlayerChannel2 - 1])
                            return false;

                        IsSet[_devices[dev].Inputs[inp].PlayerChannel2 - 1] = true;
                    }
  
                }
            }
            return true;
        }

        private void TestDelay()
        {
            SaveMicConfig();

            if (_DelayTest == null)
                return;

            _DelaySound = CSound.PlaySound(ESounds.T440);
            _DelayTestRunning = true; 
        }

        private void GetFirstConfiguredRecordDevice(ref int Device, ref int Input)
        {
            if (_devices == null)
                return;

            if (CConfig.MicConfig == null)
                return;
            
            for (int i = 0; i < _devices.Length; i++)
            {
                if (_devices[i].Name == CConfig.MicConfig[0].DeviceName && _devices[i].Driver == CConfig.MicConfig[0].DeviceDriver)
                {
                    for (int j = 0; j < _devices[i].Inputs.Count; j++)
                    {
                        if (_devices[i].Inputs[j].Name == CConfig.MicConfig[0].InputName)
                        {
                            if (CConfig.MicConfig[0].Channel > 0)
                            {
                                Device = i;
                                Input = j;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
