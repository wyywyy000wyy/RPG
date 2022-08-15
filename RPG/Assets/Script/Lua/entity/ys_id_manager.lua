local ys_id_manager = class("ys_id_manager", T.ys_manager_base_game)

function ys_id_manager:constructor()
    self.id_poor = 1
end

function ys_id_manager:new_id()
    local id = T.ys_id()
    id._id = self.id_poor
    self.id_poor = self.id_poor + 1
    return id
end

