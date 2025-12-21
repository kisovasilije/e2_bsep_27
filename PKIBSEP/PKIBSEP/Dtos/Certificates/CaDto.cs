namespace PKIBSEP.Dtos.Certificates;

public record CaDto(
    int Id,
    string SubjectDn,
    DateTime NotBefore,
    DateTime NotAfter);
