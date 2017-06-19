﻿using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Organizer;
using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser;
using System.Text;
using System.IO;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface IAlbumService
    {
        Album GetAlbum(int albumid);
        List<Album> GetAlbums(IEnumerable<int> albumIds);
        List<Album> GetAlbumsByArtist(int artistId);
        Album AddAlbum(Album newAlbum);
        Album FindById(string spotifyId);
        Album FindByTitleInexact(string title);
        void DeleteAlbum(int albumId, bool deleteFiles);
        List<Album> GetAllAlbums();
        Album UpdateAlbum(Album album);
        List<Album> UpdateAlbums(List<Album> album);
        void InsertMany(List<Album> albums);
        void UpdateMany(List<Album> albums);
        void DeleteMany(List<Album> albums);
        bool AlbumPathExists(string folder);
        void RemoveAddOptions(Album album);
    }

    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackService _trackService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly Logger _logger;

        public AlbumService(IAlbumRepository albumRepository,
                            IEventAggregator eventAggregator,
                            ITrackService trackService,
                            IBuildFileNames fileNameBuilder,
                            Logger logger)
        {
            _albumRepository = albumRepository;
            _eventAggregator = eventAggregator;
            _trackService = trackService;
            _fileNameBuilder = fileNameBuilder;
            _logger = logger;
        }

        public Album AddAlbum(Album newAlbum)
        {
            _albumRepository.Insert(newAlbum);
            _eventAggregator.PublishEvent(new AlbumAddedEvent(GetAlbum(newAlbum.Id)));

            return newAlbum;
        }

        public bool AlbumPathExists(string folder)
        {
            return _albumRepository.AlbumPathExists(folder);
        }

        public void DeleteAlbum(int albumId, bool deleteFiles)
        {
            var album = _albumRepository.Get(albumId);
            _albumRepository.Delete(albumId);
            _eventAggregator.PublishEvent(new AlbumDeletedEvent(album, deleteFiles));
        }

        public Album FindById(string spotifyId)
        {
            return _albumRepository.FindById(spotifyId);
        }



        public Album FindByTitleInexact(string title)
        {
            throw new NotImplementedException();
        }

        public List<Album> GetAllAlbums()
        {
            return _albumRepository.All().ToList();
        }

        public Album GetAlbum(int albumId)
        {
            return _albumRepository.Get(albumId);
        }

        public List<Album> GetAlbums(IEnumerable<int> albumIds)
        {
            return _albumRepository.Get(albumIds).ToList();
        }

        public List<Album> GetAlbumsByArtist(int artistId)
        {
            return _albumRepository.GetAlbums(artistId).ToList();
        }

        public void RemoveAddOptions(Album album)
        {
            _albumRepository.SetFields(album, s => s.AddOptions);
        }

        public void InsertMany(List<Album> albums)
        {
            _albumRepository.InsertMany(albums);
        }

        public void UpdateMany(List<Album> albums)
        {
            _albumRepository.UpdateMany(albums);
        }

        public void DeleteMany(List<Album> albums)
        {
            _albumRepository.DeleteMany(albums);
        }

        public Album UpdateAlbum(Album album)
        {
            var storedAlbum = GetAlbum(album.Id); // Is it Id or iTunesId? 

            var updatedAlbum = _albumRepository.Update(album);
            _eventAggregator.PublishEvent(new AlbumEditedEvent(updatedAlbum, storedAlbum));

            return updatedAlbum;
        }

        public List<Album> UpdateAlbums(List<Album> album)
        {
            _logger.Debug("Updating {0} album", album.Count);

            _albumRepository.UpdateMany(album);
            _logger.Debug("{0} albums updated", album.Count);

            return album;
        }
    }
}
