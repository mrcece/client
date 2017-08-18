--
-- Created by HuangQiang on 2017/6/12.
--

-- 简化UnityEngine 类全名
SceneManager = UnityEngine.SceneManagement.SceneManager
GameObject = UnityEngine.GameObject


function require_and_clear_loaded(module)
	local r = require(module)
	package.loaded[module] = nil
	return r
end

