namespace SoraBot_v2.WebApiModels
{
    public class WebUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Discriminator { get; set; }
        public string AvatarUrl { get; set; }
    }
}