local require = require
require "global"

function Main()

	for _, module in ipairs(require_and_clear_loaded("modules")) do
		--print("== load ", module, "begin")
		local m = require(module)
		--print("== load ", module, "end")
		m.Init()
	end
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end
