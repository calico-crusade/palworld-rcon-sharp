CREATE TABLE IF NOT EXISTS rcon_player (
	id INTEGER PRIMARY KEY,
	
	server_id INTEGER NOT NULL,
	player_id TEXT NOT NULL,
	steam_id TEXT NOT NULL,
	username TEXT NOT NULL,
	discord_id TEXT,
	last_seen TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

	created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_at TEXT,

	UNIQUE (server_id, player_id),
	FOREIGN KEY (server_id) REFERENCES rcon_server(id)
)