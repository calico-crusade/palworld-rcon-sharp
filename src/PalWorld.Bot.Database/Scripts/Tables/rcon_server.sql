CREATE TABLE IF NOT EXISTS rcon_server (
	id INTEGER PRIMARY KEY,
	
	owner_id TEXT NOT NULL,
	server_host TEXT NOT NULL,
	server_port INTEGER NOT NULL,
	password TEXT NOT NULL,
	timeout_seconds INTEGER NOT NULL DEFAULT 20,
	max_retries INTEGER NOT NULL DEFAULT 3,
	server_name TEXT,
	server_version TEXT,
	admin_ids TEXT NOT NULL DEFAULT '[]',
	poll_enabled INTEGER NOT NULL DEFAULT 1,
	poll_seconds INTEGER NOT NULL DEFAULT 30,
	last_players_poll TEXT,
	last_info_poll TEXT,
	
	created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_at TEXT,

	UNIQUE (server_host, server_port)
)