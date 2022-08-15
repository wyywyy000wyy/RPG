local ys_object = class("ys_object")


function ys_object:constructor()
    self._id = G_ID:new_id()
    self._components = T.ys_component_manager(self)
    self._properties = T.ys_object_properties(self)
end

function ys_object:add_component(type, ...)
    return self._components:add_component(type, ...)
end

function ys_object:get_component(type)
    return self._components:get_component(type)
end