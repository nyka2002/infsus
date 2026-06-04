package hr.fer.rentio.rentio_bpmn.delegates;

import org.camunda.bpm.engine.delegate.DelegateExecution;
import org.camunda.bpm.engine.delegate.JavaDelegate;
import org.springframework.stereotype.Component;

@Component("provjeriDostupnost")
public class ProvjeraDostupnostiDelegate implements JavaDelegate {

    @Override
    public void execute(DelegateExecution execution) throws Exception {
        String apartmanId = (String) execution.getVariable("apartmanId");
        String datumDolaska = (String) execution.getVariable("datumDolaska");
        String datumOdlaska = (String) execution.getVariable("datumOdlaska");

        // Simulacija provjere dostupnosti
        // U pravoj aplikaciji ovdje bi se pitala baza podataka
        boolean dostupno = !"zauzet".equalsIgnoreCase(apartmanId);

        execution.setVariable("dostupno", dostupno);

        System.out.println("Provjera dostupnosti: apartman=" + apartmanId
                + ", od=" + datumDolaska + ", do=" + datumOdlaska
                + " → " + (dostupno ? "DOSTUPNO" : "ZAUZETO"));
    }
}
