

require("core.module")
require("core.class")
require("core.serpent")
require("core.logger")
require("global_require")
require("core.ys_loader")

logger.print("DrawTest Lua loaded ")
-- require("TestSwitchDraw3")

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




