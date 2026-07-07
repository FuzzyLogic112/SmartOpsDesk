CREATE TABLE IF NOT EXISTS tickets (
    id INTEGER PRIMARY KEY,
    title TEXT NOT NULL,
    requester TEXT NOT NULL,
    department TEXT NOT NULL,
    description TEXT NOT NULL,
    category TEXT NOT NULL,
    priority TEXT NOT NULL,
    suggested_owner TEXT NOT NULL,
    status TEXT NOT NULL,
    ai_reason TEXT NOT NULL,
    handling_advice TEXT NOT NULL,
    confidence DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    due_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS approval_records (
    id BIGSERIAL PRIMARY KEY,
    ticket_id INTEGER NOT NULL,
    time TIMESTAMP NOT NULL,
    operator TEXT NOT NULL,
    role TEXT NOT NULL,
    action TEXT NOT NULL,
    comment TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ticket_attachments (
    id BIGSERIAL PRIMARY KEY,
    ticket_id INTEGER NOT NULL,
    file_name TEXT NOT NULL,
    stored_path TEXT NOT NULL,
    uploaded_by TEXT NOT NULL,
    uploaded_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS deployment_logs (
    id BIGSERIAL PRIMARY KEY,
    ticket_id INTEGER NOT NULL,
    time TIMESTAMP NOT NULL,
    operator TEXT NOT NULL,
    environment TEXT NOT NULL,
    content TEXT NOT NULL
);
