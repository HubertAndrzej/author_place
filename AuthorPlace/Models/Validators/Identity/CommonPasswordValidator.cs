﻿using Microsoft.AspNetCore.Identity;

namespace AuthorPlace.Models.Validators.Identity;

public class CommonPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    private readonly string[] commons;
    public CommonPasswordValidator()
    {
        this.commons = new[]
        {
            "000000", "111111", "123123", "123321", "1234", "12345", "123456", "1234567", "12345678", "123456789", "1234567890", "123abc", "654321", "666666", "696969", "aaaaaa", "abc123", "alberto", "alejandra", "alejandro", "amanda", "andrea", "angel", "angels", "anthony", "asdf", "asdfasdf", "ashley", "babygirl", "baseball", "basketball", "beatriz", "blahblah", "bubbles", "buster", "butterfly", "carlos", "charlie", "cheese", "chocolate", "computer", "daniel", "diablo", "dragon", "elite", "estrella", "flower", "football", "forum", "freedom", "friends", "fuckyou", "hello", "hunter", "iloveu", "iloveyou", "internet", "jennifer", "jessica", "jesus", "jordan", "joshua", "justin", "killer", "letmein", "liverpool", "lovely", "loveme", "loveyou", "master", "matrix", "merlin", "monkey", "mustang", "nicole", "nothing", "number1", "pass", "passport", "password", "password1", "playboy", "pokemon", "pretty", "princess", "purple", "pussy", "qazwsx", "qwerty", "roberto", "sebastian", "secret", "shadow", "shit", "soccer", "starwars", "sunshine", "superman", "tequiero", "test", "testing", "trustno1", "tweety", "welcome", "westside", "whatever", "windows", "writer", "zxcvbnm", "zxczxc"
        };
    }

    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        IdentityResult result;
        if (commons.Any(common => password.Contains(common, StringComparison.CurrentCultureIgnoreCase)))
        {
            result = IdentityResult.Failed(new IdentityError { Description = "The password is too common." });
        }
        else
        {
            result = IdentityResult.Success;
        }
        return Task.FromResult(result);
    }
}