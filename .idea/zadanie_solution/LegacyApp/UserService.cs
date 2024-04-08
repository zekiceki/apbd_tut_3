using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository;
        private readonly UserCreditService _userCreditService;

        public UserService()
        {
            _clientRepository = new ClientRepository();
            _userCreditService = new UserCreditService();
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidFirstName(firstName) || !IsValidLastName(lastName) || !IsValidEmail(email) || IsUnderAge(dateOfBirth))
                return false;

            var client = _clientRepository.GetById(clientId);
            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);
            SetCreditLimit(client, user);

            if (!HasSufficientCredit(user))
                return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidFirstName(string firstName)
        {
            return !string.IsNullOrEmpty(firstName);
        }

        private bool IsValidLastName(string lastName)
        {
            return !string.IsNullOrEmpty(lastName);
        }

        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private bool IsUnderAge(DateTime dateOfBirth)
        {
            return DateTime.Now.Subtract(dateOfBirth).TotalDays < 7665; // 21 years
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private void SetCreditLimit(Client client, User user)
        {
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                user.HasCreditLimit = true;
                var creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = client.Type == "ImportantClient" ? creditLimit * 2 : creditLimit;
            }
        }

        private bool HasSufficientCredit(User user)
        {
            return !user.HasCreditLimit || user.CreditLimit >= 500;
        }
    }
}
