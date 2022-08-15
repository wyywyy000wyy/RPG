T.ys_component = class("ys_component")
local ys_component = T.ys_component

function ys_component:constructor(obj)
    self._obj = obj
end

function ys_component:register_event()
end

function ys_component:unregister_event()
end

function ys_component:enable()
end

function ys_component:disable()
end

function ys_component:on_start()
end

function ys_component:on_enable()
end

function ys_component:on_disable()
end