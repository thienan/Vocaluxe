﻿#region license
// This file is part of Vocaluxe.
// 
// Vocaluxe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Vocaluxe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Vocaluxe. If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace VocaluxeLib.Menu
{
    public enum EType
    {
        Background,
        Button,
        SelectSlide,
        Text,
        Static,
        SongMenu,
        Lyric,
        SingNote,
        NameSelection,
        Equalizer,
        Playlist,
        ParticleEffect
    }

    class CInteraction
    {
        public readonly int Num;
        public readonly EType Type;

        public bool ThemeEditorOnly
        {
            get
            {
                return Type == EType.Background ||
                       Type == EType.NameSelection ||
                       Type == EType.Text ||
                       Type == EType.Static ||
                       Type == EType.SongMenu ||
                       Type == EType.Lyric ||
                       Type == EType.SingNote ||
                       Type == EType.Equalizer ||
                       Type == EType.Playlist;
            }
        }

        public bool DrawAsForeground
        {
            get
            {
                return Type == EType.Button ||
                       Type == EType.SelectSlide ||
                       Type == EType.Static ||
                       Type == EType.NameSelection ||
                       Type == EType.Text ||
                       Type == EType.SongMenu ||
                       Type == EType.Equalizer ||
                       Type == EType.Playlist ||
                       Type == EType.ParticleEffect;
            }
        }

        public CInteraction(int num, EType type)
        {
            Num = num;
            Type = type;
        }
    }
}