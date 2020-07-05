namespace MedReminder.Services {
    public enum ZustandChat {
        NichtBekannt = 0,
        WarteAufName = 1,
        WarteAufUhrzeit = 2,
        Fertig = 3,
        WarteAufBestaetigungDerErinnerung = 4,
        ResetStart = 5
    }

    public enum ErinnerungsTyp {
        ErsteErinnerung = 0,
        ZusaetzlicheErinnerung = 1,
        UeberfaelligeErinnerung = 2
    }
}