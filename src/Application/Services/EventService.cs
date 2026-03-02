using AutoMapper;
using EventManagement.Application.DTOs;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Requests;
using EventManagement.Data.Interfaces;
using EventManagement.Models;

namespace EventManagement.Application.Services
{
    /// <summary>
    /// Сервис для работы с мероприятиями
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public EventDto Add(AddEventRequest addEventRequest)
        {
            var newEvent = _mapper.Map<Event>(addEventRequest);
            var addedEvent = _eventRepository.Add(newEvent);
            return _mapper.Map<EventDto>(addedEvent);
        }

        public void Delete(Guid id)
        {
            _eventRepository.Delete(id);
        }

        public List<EventDto> GetAll()
        {
            var events = _eventRepository.GetAll();
            return events
                .Select(_mapper.Map<EventDto>)
                .ToList();
        }

        public EventDto GetById(Guid id)
        {
            var eventItem = _eventRepository.GetById(id);
            if(eventItem == null)
            {
                return null;
            }
            return _mapper.Map<EventDto>(eventItem);
        }

        public void Update(Guid id, UpdateEventRequest updateEventRequest)
        {
            var updatedEvent = _mapper.Map<Event>(updateEventRequest);
            updatedEvent.Id = id;
            _eventRepository.Update(updatedEvent);
        }
    }
}
