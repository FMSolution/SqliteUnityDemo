/**
 * Socket.IO Demo Server
 * 
 * Install dependencies:  npm install
 * Run:                   node server.js
 * 
 * Events handled:
 *   client → server:  "chat_message"  { user, text }
 *   client → server:  "set_username"  { username }
 *   server → client:  "chat_message"  { user, text, timestamp }
 *   server → client:  "user_joined"   { username, onlineCount }
 *   server → client:  "user_left"     { username, onlineCount }
 *   server → client:  "online_count"  { count }
 *   server → all:     "server_info"   { message }
 */

const { createServer } = require("http");
const { Server }       = require("socket.io");

const PORT = 3000;
const httpServer = createServer();
const io = new Server(httpServer, {
    cors: { origin: "*", methods: ["GET", "POST"] }
});

// Track connected users: socketId → username
const users = new Map();

io.on("connection", (socket) => {
    console.log(`[+] Socket connected: ${socket.id}`);

    // Default username until client sets one
    users.set(socket.id, `Guest_${socket.id.substring(0, 5)}`);

    // Notify the new client of current online count
    socket.emit("online_count", { count: users.size });

    // ── set_username ──────────────────────────────────────────────────────────
    socket.on("set_username", (data) => {
        const oldName = users.get(socket.id);
        const newName = (data.username || "").trim() || oldName;
        users.set(socket.id, newName);

        console.log(`[~] ${oldName} → ${newName}`);

        // Broadcast join to everyone
        io.emit("user_joined", {
            username:    newName,
            onlineCount: users.size
        });

        io.emit("online_count", { count: users.size });
    });

    // ── chat_message ──────────────────────────────────────────────────────────
    socket.on("chat_message", (data) => {
        const username = users.get(socket.id) || "Unknown";
        const text     = (data.text || "").trim();
        if (!text) return;

        const payload = {
            user:      username,
            text:      text,
            timestamp: new Date().toLocaleTimeString()
        };

        console.log(`[MSG] ${username}: ${text}`);

        // Broadcast to ALL clients (including sender)
        io.emit("chat_message", payload);
    });

    // ── disconnect ────────────────────────────────────────────────────────────
    socket.on("disconnect", () => {
        const username = users.get(socket.id) || socket.id;
        users.delete(socket.id);

        console.log(`[-] ${username} disconnected. Online: ${users.size}`);

        io.emit("user_left", {
            username:    username,
            onlineCount: users.size
        });

        io.emit("online_count", { count: users.size });
    });
});

httpServer.listen(PORT, () => {
    console.log(`✅ Socket.IO server running on http://localhost:${PORT}`);
    console.log(`   Press Ctrl+C to stop.`);
});
