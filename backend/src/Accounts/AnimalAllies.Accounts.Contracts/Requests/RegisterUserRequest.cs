﻿namespace AnimalAllies.Accounts.Contracts.Requests;



public record RegisterUserRequest(
    string Email,
    string UserName,
    string FirstName,
    string SecondName,
    string Patronymic,
    string Password);
