

require("framework/core/module")
require("framework/core/class")
require("framework/core/serpent")
require("framework/core/logger")
require("framework/global_require")
require("framework/core/ys_loader")

logger.print("DrawTest Lua loaded ")
-- require("framework.TestSwitchDraw3")

local wait_frame = 0

local cor = coroutine.create(function()
    for i = 1, wait_frame do
        coroutine.yield()
    end
end)

for i = 1, wait_frame do
    coroutine.resume(cor)
end

coroutine.resume(cor)
logger.print(coroutine.status(cor)) -- suspended
logger.print(coroutine.running())







logger.print("game running!!")




