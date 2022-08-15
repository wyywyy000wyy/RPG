local ys_component_manager = class("ys_component_manager")

function ys_component_manager:constructor(obj)
    self._obj = obj
    self._components = {}
end

function ys_component_manager:add_component(type, ...)
    local comp = type(self._obj, ...)
    table.insert(self._components, comp)
    return comp
end

function ys_component_manager:get_component(type)
    for _, comp in ipairs(self._components) do
        if comp:is_a(type) then
            return comp
        end
    end
    return nil
end