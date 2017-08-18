--
-- Created by HuangQiang on 2017/6/11.
--


local inited = {}

function inited.Init()
	print("== inited finish")
	collectgarbage("collect")
	SceneManager.LoadScene("main")
end

return inited