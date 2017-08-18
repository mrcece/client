return {
	urls = {
		"file:///d:/update",
		"file:///e:/update",
	},
	
	sources = 
	{
		{
			name = "ios_dev",
			type = "dev",
			platform="ios",
			app_version = 101,
			compatible_app_version = {100, 199},
			path = "ios_debug",
			manifest = { resource_version =500, md5md5="abcdefg"},
		},
		{
			name = "ios_release",
			type = "release",
			platform="ios",
			app_version = 101,
			compatible_app_version = {100, 199},
			path = "ios",
			manifest = { resource_version=500, md5md5="abcdefg"},
		},
		{
			name = "ios_review",
			type = "review",
			platform="ios",
			app_version = 201,
			compatible_app_version = {200, 299},
			path = "review",
			manifest = { resource_version=110, md5md5="87309212149a0cedf64bba95f95a7181"},
		},		
	}
}