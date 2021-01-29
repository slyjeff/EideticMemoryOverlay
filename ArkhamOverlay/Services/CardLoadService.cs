using ArkhamOverlay.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArkhamOverlay.Services {
    public class CardLoadService {
        private readonly AppData _appData;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly ArkhamDbService _arkhamDbService;
        private readonly LocalCardsService _localCardsService;
        private readonly LoggingService _logger;

        public CardLoadService(ArkhamDbService arkhamDbService, LocalCardsService localCardsService, AppData appData, LoadingStatusService loadingStatusService, LoggingService loggingService) {
            _arkhamDbService = arkhamDbService;
            _localCardsService = localCardsService;
            _appData = appData;
            _loadingStatusService = loadingStatusService;
            _logger = loggingService;
        }

        internal void RegisterEvents() {
            _appData.Game.PlayersChanged += LoadPlayerCards;
            _appData.Game.EncounterSetsChanged += LoadEncounterCards;
        }

        private void LoadEncounterCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _loadingStatusService.ReportEncounterCardsStatus(Status.LoadingCards);
                _logger.LogMessage("Loading encounter cards.");
                try {
                    _appData.Game.ScenarioCards.Loading = true;
                    _appData.Game.LocationCards.Loading = true;
                    _appData.Game.EncounterDeckCards.Loading = true;

                    var cards = _arkhamDbService.LoadEncounterCards();
                    cards.AddRange(_localCardsService.LoadEncounterCards());

                    _loadingStatusService.ReportEncounterCardsStatus(Status.Finished);

                    var scenarioCards = new List<Card>();
                    var agendas = new List<Card>();
                    var acts = new List<Card>();
                    var locations = new List<Card>();
                    var treacheries = new List<Card>();
                    var enemies = new List<Card>();

                    foreach (var card in cards) {
                        switch (card.Type) {
                            case CardType.Scenario:
                                scenarioCards.Add(card);
                                break;
                            case CardType.Agenda:
                                agendas.Add(card);
                                break;
                            case CardType.Act:
                                acts.Add(card);
                                break;
                            case CardType.Location:
                                locations.Add(card);
                                break;
                            case CardType.Treachery:
                            case CardType.Enemy:
                                treacheries.Add(card);
                                break;
                            default:
                                break;
                        }
                    }

                    scenarioCards.AddRange(agendas);
                    scenarioCards.AddRange(acts);

                    _appData.Game.ScenarioCards.LoadCards(scenarioCards);
                    _appData.Game.LocationCards.LoadCards(locations);
                    _appData.Game.EncounterDeckCards.LoadCards(treacheries);
                } catch (Exception ex) {
                    _logger.LogException(ex, "Error loading encounter cards.");
                    _loadingStatusService.ReportEncounterCardsStatus(Status.Error);
                } finally {
                    _appData.Game.ScenarioCards.Loading = false;
                    _appData.Game.LocationCards.Loading = false;
                    _appData.Game.EncounterDeckCards.Loading = false;
                }
                _logger.LogMessage($"Finished loading encounter cards.");

            };
            worker.RunWorkerAsync();
        }

        private void LoadPlayerCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                foreach (var player in _appData.Game.Players) {
                    if (!string.IsNullOrEmpty(player.DeckId)) {
                        _loadingStatusService.ReportPlayerStatus(player.ID, Status.LoadingCards);
                        try {
                            _arkhamDbService.LoadPlayerCards(player);
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Finished);
                        }
                        catch (Exception ex) {
                            _logger.LogException(ex, $"Error loading player cards for player {player.ID}");
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                        }
                    }
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
