﻿namespace OrderLunch.ResponseModels
{
    public class GoogleAuthResponseModel
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
    }
}
