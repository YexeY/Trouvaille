using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Models.Communication
{
    public class AddressViewModel
    {
        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [StringLength(50)]
        public string Street { get; set; }

        [Required]
        public int StreetNumber { get; set; }

        [Required]
        public int PostalCode { get; set; }

        [Required]
        [StringLength(50)]
        public string CityName { get; set; }

        
        public static Address GetAddress(AddressViewModel model)
        {
            var cityId = Guid.NewGuid();
            return new Address()
            {
                AddressId = Guid.NewGuid(),
                Country = model.Country,
                State = model.State,
                Street = model.Street,
                StreetNumber = model.StreetNumber,
                City = new City()
                {
                    CityId = cityId,
                    Name = model.CityName,
                    PostalCode = model.PostalCode
                },
                CityId = cityId
            };
        }
        
        public AddressViewModel(Address address)
        {
            Country = address.Country;
            State = address.State;
            Street = address.Street;
            StreetNumber = address.StreetNumber;
            PostalCode = address.City.PostalCode;
            CityName = address.City.Name;
        }

        public AddressViewModel(string country, string state, string street, int streetNumber, int postalCode,
            string cityName)
        {
            Country = country;
            State = state;
            Street = street;
            StreetNumber = streetNumber;
            PostalCode = postalCode;
            CityName = cityName;
        }

        public AddressViewModel()
        {

        }
    }
}
