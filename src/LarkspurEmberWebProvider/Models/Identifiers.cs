using System;
namespace LarkspurEmberWebProvider.Models
{
    public enum UtilitiesIdentifiers
    {
        Server = 0,
        StartTime,
        LogLevel,
        Restart,
        ReloadWebGuiUrls
    }

    public enum RootIdentifiers
    {
        Identity = 0,
        Utilities,
        CodecPools,
        TxPool,
        Studios,
        Regions
    }

    public enum CodecPoolNodeIdentifiers
    {
        DisplayName = 0,
        Units
    }

    public enum PoolCodecNodeIdentifiers
    {
        SipId = 0,
        DisplayName,
        Owner,
        IsRegistered,
        IsInCall
    }

    public enum TxNodeIdentifiers
    {
        Region = 0,
        DisplayName,
        Ta,
        Nationalfeed,
        Sources
    }

    public enum TxSourceIdentifiers
    {
        StudioId = 0,
        StudioDisplayName,
        Prepare,
        OnAir,
        PrepareBool,
        OnAirBool
    }

    public enum NetIoNodeIdentifiers
    {
        ConnectedTo = 0
    }

    public enum CodecSlotNodeIdentifiers
    {
        SipId = 0,
        DisplayName,
        IsOnAir, // True om kodarslotten är i sändning. 
        IsInCall,
        IsInPhoneCall,
        IsOutgoingCall,
        ConnectedToSipId,
        ConnectedToDisplayName,
        ConnectedToLocation,
        ConnectedToDisplayNameAndLocation,
        // Funktioner
        SetGpo0,
        SetGpo1,
        Hangup,
        ConnectedToSetGpo0,
        ConnectedToSetGpo1,
        ConnectedToInputs,
        ConnectedToTurnInputsOn,
        ConnectedToTurnInputsOff
    }

    public enum CodecSlotInputIdentifiers
    {
        InputNumber,
        IsEnabled,
        CurrentGain,
        RefreshValues,
        Enable,
        IncreaseGain,
        DecreaseGain,
        ControlThis
    }

    public enum StudioNodeIdentifiers
    {
        StudioId = 0,
        DisplayName,
        Region,
        ModifyRegion,
        LoggedInUser,
        LoggedInDisplayName,
        Type,
        Remoted,
        InfoServiceOnAir, // Trafik
        HostMicOn,
        GuestMicOn,
        InMaintenance,
        OnAir,
        Prepare,
        PrivateMode,
        ActiveStudioConnection,
        Recording,
        WebGuiUrls,
        CodecSlots,
        NetIos
    }

    public enum DisplayNodeIdentifiers
    {
        PlaylistUrl = 0,
        JinglesUrl,
        Player01Url,
        Player02Url,
        Player03Url,
        PrelistenUrl,
        Dialer01Url,
        Dialer02Url,
        Dialer03Url,
        Dialer04Url,
        StudioSignageUrl,
        ReadonlyPlaylistUrl,
        RemoteConnectorUrl,
        SidekickScreenUrl,
        DeskPlaylistUrl,
        NetIo01Url,
        NetIo02Url,
        NetIo03Url,
        NetIo04Url
    }

    public enum RegionNodeIdentifiers
    {
        Name = 0
    }
}