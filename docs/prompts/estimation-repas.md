# Prompt : estimation des kcal d'un repas

Repris de l'artefact (`promptRepas` et `nettoyerRepas`). Les éléments entre `{accolades}` sont à remplir par l'API.

- Modèle : niveau « default » (Sonnet), avec vision quand il y a des images.
- Entrées : le texte saisi, 0 à 3 images (photo du plat, capture d'une recette, étiquette nutritionnelle), le moment du repas.
- Côté API : redimensionner les images (≈ 1,2 mégapixel, JPEG) avant l'envoi, refuser au-delà de 20 Mo.
- Le résultat n'est **pas** enregistré tout seul : l'utilisateur ajuste les portions puis valide.

## Texte envoyé

```text
Tu es diététicien. Estime les calories et les macronutriments d'un repas pour un carnet alimentaire.

Personne : {homme | femme} adulte{ de {âge} ans}, environ {poids} kg, qui fait de la musculation et veut perdre du gras. Elle vit en France : prends des portions françaises habituelles.
{si images : "Il joint {une image | n images} : photo du plat, capture d'une recette ou étiquette nutritionnelle. Lis {l'image | chaque image} attentivement (ingrédients, quantités, nombre de portions, valeurs nutritionnelles)."}
Moment du repas : {petit-déjeuner | déjeuner | en-cas | dîner}.
Ce qu'elle a écrit : {« texte » (1 500 caractères max) | rien, utilise seulement l'image.}

Règles :
- Si c'est une recette pour plusieurs personnes, donne les valeurs pour UNE portion et mets le nombre de portions de la recette dans "portionsRecette".
- Si une quantité est écrite (grammes, nombre de pièces, « grosse assiette »), respecte-la. Sinon, prends une portion adulte normale et écris laquelle dans "portion".
- Si une étiquette nutritionnelle est visible, utilise ses valeurs.
- Compte l'huile, le beurre, le fromage et les sauces habituels de la préparation, et les boissons citées.
- Arrondis les kcal à 10 près et les macronutriments au gramme.

Réponds uniquement avec un objet JSON de cette forme :
{"nom": "nom court du repas, 2 à 6 mots", "portion": "la quantité retenue pour 1 portion, ex. 1 assiette d'environ 350 g", "portionsRecette": 1, "elements": [{"nom": "Pâtes cuites", "quantite": "200 g", "kcal": 310}], "kcal": 650, "proteines": 30, "glucides": 80, "lipides": 20, "fibres": 6, "confiance": "moyenne", "remarque": "1 phrase courte sur ce qui fait le plus varier le total, ou une chaîne vide"}
"kcal", "proteines", "glucides", "lipides" et "fibres" sont les totaux pour 1 portion, et "kcal" est la somme des éléments. "confiance" vaut "haute", "moyenne" ou "faible". Au plus 10 éléments.
Si ni le texte ni l'image ne décrivent un aliment ou une recette, réponds seulement {"erreur": "1 phrase qui dit ce qu'il manque"}.
```

## Nettoyage de la réponse

- Si `erreur` est présent et qu'il n'y a pas de `kcal` > 0 : renvoyer l'erreur à l'utilisateur (« Décris ton plat ou ajoute une photo »).
- `elements` : 12 au maximum, `nom` ≤ 60, `quantite` ≤ 40, `kcal` entre 0 et 4 000.
- `kcal` entre 0 et 6 000 ; s'il manque ou vaut 0, prendre la somme des éléments ; rejeter si toujours ≤ 0.
- `proteines` ≤ 400, `glucides` ≤ 800, `lipides` ≤ 400 (sinon 0), arrondis.
- `fibres` ≤ 150, arrondi ; **absent ou invalide → NULL** (non estimé), jamais 0.
- `portionsRecette` entre 1 et 20, `confiance` dans {haute, moyenne, faible} (sinon moyenne), `remarque` ≤ 220.
- Côté base, `confiance` haute/moyenne/faible devient `high`/`medium`/`low`.
