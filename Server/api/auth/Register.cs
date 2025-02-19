﻿using Server.api.auth;

namespace Server.api.auth
{
    public class Register
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; } 
    }
}