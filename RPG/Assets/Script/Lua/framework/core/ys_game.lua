T.ys_id_manager = class("ys_game")
local ys_game = T.ys_id_manager

function ys_game:constructor()
    self._id_manager = T.ys_id_manager()
    G_GAME = self
    G_ID = self._id_manager
end

function ys_game:destructor()
    G_GAME = nil
end