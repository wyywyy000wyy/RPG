T.ui_group = class("ui_group")
local ui_group = T.ui_group

function ui_group:constructor(name, windows)
    self.windows = T.map_list()
end



function ui_group:on_active()
end

function ui_group:on_deactive()
end