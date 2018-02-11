using System.Collections.Generic;

namespace SoraBot_v2.WebApiModels
{
    public class GuildLevels
    {
        public string LevelUpMessage { get; set; }
        public bool EnabledLvlUpMessage { get; set; }
        public bool SendLvlDm { get; set; }
        public List<WebLevelRole> LevelRoles { get; set; }= new List<WebLevelRole>();
    }

    public class WebLevelRole
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int RequiredLevel { get; set; }
        public string RGBColor { get; set; }
        public bool Banned { get; set; }
    }

    public class GuildLevelEdit
    {
	    public string GuildId { get; set; }
	    public bool EnableAnn { get; set; }
	    public bool SendDm { get; set; }
	    public string LvlUpMsg { get; set; }
	    public string[] BannedRoles { get; set; }
	    public List<LevelRole> Roles { get; set; }
	    public string UserId { get; set; }
    }

	public class LevelRole
	{
		public string RoleId { get; set; }
		public int LvlReq { get; set; }
	}
}