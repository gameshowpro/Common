﻿// (C) Barjonas LLC 2018

using Color = System.Windows.Media.Color;

namespace GameshowPro.Common.Model.Lights;

/// <summary>
/// Represents a type of channel which can belong to a fixture.  
/// In this implementation, the primary color is the only distinguishing feature between types, but other charactaristics could be added.
/// </summary>
public class FixtureChannelType : ObservableClass
{
    private Color _primary;
    [DataMember, DefaultValue("#00000000")]
    public Color Primary
    {
        get { return _primary; }
        set { SetProperty(ref _primary, value); }
    }
}
