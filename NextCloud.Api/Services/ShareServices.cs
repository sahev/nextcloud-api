namespace NextCloud.Api.Services
{
    public class ShareServices
    {
        public static async Task<Share> CreatePublicShare(NextCloudService nextCloudService, string path)
        {
            var share = await Share.Create(nextCloudService, new ShareCreateInfo
            {
                path = path,
                shareType = (int)ShareType.PublicLink
            });

            var host = new Uri(share.url);

            var publicHref = $"{host.Scheme}://{host.Authority}/apps/files_sharing/publicpreview/{host.Segments[2]}?file=/&x=1920&y=1080&a=true";
            var publicPreview = $"{host.Scheme}://{host.Authority}/apps/files_sharing/publicpreview/{host.Segments[2]}";

            share.public_href = publicHref;
            share.public_preview = publicPreview;

            return share;
        }
    }
}
