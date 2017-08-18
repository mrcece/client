return {
	urls = {
		"file:///d:/update/",
		"file:///e:/update/",
	},
	
	sources = 
	{
		{
			name = "ios_debug",
			platform="ios",
			version = { major=1, minor=12},
			path = "ios_debug",
			manifest = { resource=500, md5md5="abcdefg"},
		},
		{
			name = "ios_release",
			platform="ios",
			version = { major=1, minor=12},
			path = "ios",
			manifest = { resource=500, md5md5="abcdefg"},
		},
		{
			name = "ios_release",
			platform="editor",
			version = { major=1, minor=12},
			path = "editor",
			manifest = { resource=106, md5md5="02b88508d3f71453ffad1ad4d9533486"},
		},
	}
}