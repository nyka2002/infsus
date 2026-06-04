package hr.fer.rentio.rentio_bpmn.controller;

import org.camunda.bpm.engine.RuntimeService;
import org.camunda.bpm.engine.TaskService;
import org.camunda.bpm.engine.task.Task;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;

import java.util.HashMap;
import java.util.Map;

@Controller
public class RezervacijaController {

    @Autowired
    private RuntimeService runtimeService;

    @Autowired
    private TaskService taskService;

    @GetMapping("/")
    public String index() {
        return "index";
    }

    // Korisnik pokreće novi proces rezervacije
    @PostMapping("/rezervacija/pokreni")
    public String pokreniProces(Model model) {
        var instance = runtimeService.startProcessInstanceByKey("rezervacija-proces");
        model.addAttribute("processInstanceId", instance.getId());
        return "status";
    }

    // Korisnik šalje podatke rezervacije (popunjava User Task kroz Tasklist,
    // ali ovdje simuliramo slanje poruke s podacima)
    @PostMapping("/rezervacija/podaci")
    public String posaljiPodatke(
            @RequestParam String processInstanceId,
            @RequestParam String apartmanId,
            @RequestParam String datumDolaska,
            @RequestParam String datumOdlaska,
            Model model) {

        Task task = taskService.createTaskQuery()
                .processInstanceId(processInstanceId)
                .singleResult();

        if (task == null) {
            model.addAttribute("processInstanceId", processInstanceId);
            model.addAttribute("poruka", "Greška: proces nije u fazi unosa podataka.");
            return "status";
        }

        Map<String, Object> varijable = new HashMap<>();
        varijable.put("apartmanId", apartmanId);
        varijable.put("datumDolaska", datumDolaska);
        varijable.put("datumOdlaska", datumOdlaska);

        // Završi User Task — proces nastavlja na Service Task
        taskService.complete(task.getId(), varijable);

        model.addAttribute("processInstanceId", processInstanceId);
        model.addAttribute("poruka", "Podaci poslani! Provjera dostupnosti u tijeku...");
        return "status";
    }

    // Simulacija potvrde plaćanja — šalje poruku procesu
    @PostMapping("/rezervacija/plati")
    public String potvrdiPlacanje(
            @RequestParam String processInstanceId,
            Model model) {

        try {
            runtimeService.createMessageCorrelation("PlacanjePotvrdeno")
                    .correlate();
            model.addAttribute("poruka", "Plaćanje potvrđeno! Rezervacija je završena.");
        } catch (Exception e) {
            model.addAttribute("poruka", "Greška: nema aktivnog procesa koji čeka plaćanje.");
        }

        model.addAttribute("processInstanceId", processInstanceId);
        return "status";
    }
}
