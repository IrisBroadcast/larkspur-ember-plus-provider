using System.Collections.Generic;

namespace LarkspurEmberWebProvider.Models
{
    public class TxSource
    {
        public string SourceId { get; set; }
        public string StudioId { get; set; }
    }

    public class Tx
    {
        public string NodeIdentifier { get; set; }
        public string DisplayName { get; set; }
        public string RegionName { get; set; }
        public List<TxSource> Sources { get; set; }
    }

    public class PoolCodec
    {
        public string Identifier { get; set; }
        public Unit Unit { get; set; }
    }

    public class Unit
    {
        public string Identifier { get; set; }
        public string SipId { get; set; }
    }

    public class CodecPool
    {
        public string Name { get; set; }
        public List<Unit> Units { get; set; }
    }

    public class CodecSlot
    {
        public string Identifier { get; set; }
        public string DefaultSipId { get; set; }
    }

    public class Region
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
    }

    public class WebGuiUrls
    {
        public string Identifier { get; set; }
        public string PlaylistUrl { get; set; }
        public string JinglesUrl { get; set; }
        public string Player01Url { get; set; }
        public string Player02Url { get; set; }
        public string Player03Url { get; set; }
        public string PrelistenUrl { get; set; }
        public string Dialer01Url { get; set; }
        public string Dialer02Url { get; set; }
        public string Dialer03Url { get; set; }
        public string Dialer04Url { get; set; }
        public string RemoteConnectorUrl { get; set; }
        public string StudioSignageUrl { get; set; }
        public string ReadonlyPlaylistUrl { get; set; }
        public string SidekickScreenUrl { get; set; }
        public string DeskPlaylistUrl { get; set; }
        public string NetIo01Url { get; set; }
        public string NetIo02Url { get; set; }
        public string NetIo03Url { get; set; }
        public string NetIo04Url { get; set; }
    }

    public class Studio
    {
        public string NodeIdentifier { get; set; }
        public string StudioId { get; set; }
        public string DisplayName { get; set; }
        public string Region { get; set; }
        public string Type { get; set; }
        public WebGuiUrls WebGuiUrls { get; set; }
        public List<CodecSlot> CodecSlots { get; set; }
    }

    public class Configuration
    {
        public List<CodecPool> CodecPools { get; set; }
        public List<Tx> TxPool { get; set; }
        public List<Studio> Studios { get; set; }
        public List<Region> Regions { get; set; }
    }
}